using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Nerdostat.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Services
{
    public class HubManager
    {
        private int AzureStatusPinNumber = 18;

#if DEBUG
        private readonly MockPin AzureStatusLed;
#else
        private readonly OutputPin AzureStatusLed;
#endif
        private readonly Thermostat Thermo;
        private readonly Configuration Config;
        private readonly ILogger log;

        private static volatile DeviceClient client;
        private static volatile ConnectionStatus deviceStatus;
        private static bool IsDeviceConnected => deviceStatus == ConnectionStatus.Connected;

        private SemaphoreSlim deviceSemaphore = new(1, 1);

        private ConcurrentQueue<APIMessage> skippedMessages;

        public HubManager(Configuration _config, Thermostat _thermo, ILogger<HubManager> _log)
        {
            Config = _config;
            Thermo = _thermo;
            log = _log;

            AzureStatusLed = new(AzureStatusPinNumber, log, "AzureStatusLed");
            skippedMessages = new();
        }

        public async Task Initialize()
        {
            try
            {
                if (ShouldClientBeInitialized(deviceStatus))
                {
                    await deviceSemaphore.WaitAsync();
                    if (ShouldClientBeInitialized(deviceStatus))
                    {
                        if (client != null)
                        {
                            await client.DisposeAsync();
                        }

                        client = DeviceClient.CreateFromConnectionString(Config.IotHubConnectionString, TransportType.Mqtt);

                        var retryPolicy = new ExponentialBackoff(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
                        client.SetRetryPolicy(retryPolicy);
                        client.OperationTimeoutInMilliseconds = (uint)(((Config.Interval * 60) - 30) * 1000);
                        client.SetConnectionStatusChangesHandler(ConnectionStatusChangedAsync);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
            finally
            {
                deviceSemaphore.Release();
            }
            // retry transient
            await client.OpenAsync();
            await ConfigureCallbacks();
        }


        private async void ConnectionStatusChangedAsync(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            string message = $"Connection Status: {status} : {reason}";
            deviceStatus = status;
            switch (status)
            {
                case ConnectionStatus.Connected:
                    log.LogInformation(message);
                    AzureStatusLed.TurnOn();

                    while (skippedMessages.TryDequeue(out var apiMessage))
                    {
                        await SendIotMessage(apiMessage, null);
                    }
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    log.LogWarning(message);
                    AzureStatusLed.TurnOff();
                    break;
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Disabled:
                    log.LogWarning(message);
                    await Initialize();
                    break;
            }
        }

        private async Task ConfigureCallbacks()
        {
            var callbacks = new List<Task>
            {
                client.SetMethodHandlerAsync(DeviceMethods.ReadNow, RefreshThermoData, null),
                client.SetMethodHandlerAsync(DeviceMethods.SetManualSetpoint, OverrideSetpoint, null),
                client.SetMethodHandlerAsync(DeviceMethods.ClearManualSetPoint, ClearSetpoint, null),
                client.SetMethodHandlerAsync(DeviceMethods.SetAwayOn, SetAwayOn, null)
            };

            await Task.WhenAll(callbacks);

        }

        private async Task<MethodResponse> RefreshThermoData(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"{DeviceMethods.ReadNow}");
            var thermoData = await Thermo.Refresh();
            var message = new APIMessage()
            {
                Timestamp = DateTime.Now,
                Temperature = thermoData.Temperature,
                Humidity = thermoData.Humidity,
                CurrentSetpoint = thermoData.CurrentSetpoint,
                HeaterOn = Convert.ToInt64(thermoData.HeaterOn),
                IsHeaterOn = thermoData.IsHeaterOn,
                OverrideEnd = thermoData.OverrideEnd
            };

            var stringData = JsonConvert.SerializeObject(message);
            log.LogInformation("Reply " + stringData);
            var byteData = Encoding.UTF8.GetBytes(stringData);
            return new MethodResponse(byteData, 200);
        }

        private async Task<MethodResponse> OverrideSetpoint(MethodRequest methodRequest, object userContext)
        {

            log.LogInformation($"WEBR: {DeviceMethods.SetManualSetpoint}");
            var input = JsonConvert.DeserializeObject<SetPointMessage>(methodRequest.DataAsJson);
            Thermo.OverrideSetpoint(
                Convert.ToDecimal(input.Setpoint),
                Convert.ToInt32(input.Hours));
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> ClearSetpoint(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"WEBR: {DeviceMethods.ClearManualSetPoint}");

            Thermo.ReturnToProgram();
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> SetAwayOn(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"WEBR: {DeviceMethods.SetAwayOn}");

            Thermo.SetAway();
            return await RefreshThermoData(methodRequest, userContext);
        }

        public async Task SendIotMessage(APIMessage message, CancellationToken? hubOperationToken)
        {
            var messageString = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);
            var iotMessage = new Message(messageBytes)
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json"
            };

            if (Config.TestDevice)
                iotMessage.Properties.Add("testDevice", "true");

            if (ShouldClientBeInitialized(deviceStatus))
            {
                await Initialize();
            }

            CancellationTokenSource currentOpCts;
            if (hubOperationToken.HasValue)
                currentOpCts = CancellationTokenSource.CreateLinkedTokenSource(hubOperationToken.Value);
            else
            {
                currentOpCts = new CancellationTokenSource();
            }
            currentOpCts.CancelAfter(((Config.Interval * 60) - 30) * 1000);

            if (IsDeviceConnected)
            {
                var blink = AzureStatusLed.Blink((decimal)0.1, (decimal)0.1, currentOpCts.Token);
                try
                {
                    await client.SendEventAsync(iotMessage, currentOpCts.Token);
                    currentOpCts.Cancel();
                    await blink;
                    log.LogInformation($"{messageString}");
                }
                catch (OperationCanceledException canc)
                {
                    //this only runs if the process is cancelled from the main loop.
                    log.LogError("Operation cancelled", canc);
                }
                catch (Exception ex)
                {
                    log.LogError($"Generic exception", ex.ToString());
                    EnqueueMessage(message, messageString);
                }
            }
            else
            {
                EnqueueMessage(message, messageString);
            }

            currentOpCts.Dispose();
        }

        private void EnqueueMessage(APIMessage message, string messageString)
        {
            skippedMessages.Enqueue(message);
            log.LogWarning($"Enqueued message: {messageString}");
        }

        private bool ShouldClientBeInitialized(ConnectionStatus connectionStatus)
        {
            return (connectionStatus == ConnectionStatus.Disconnected || connectionStatus == ConnectionStatus.Disabled);
        }
    }
}