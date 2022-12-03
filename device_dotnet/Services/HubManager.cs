using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Nerdostat.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public async Task Initialize(CancellationToken sourceToken)
        {
            try
            {
                if (ShouldClientBeInitialized(deviceStatus))
                {
                    await deviceSemaphore.WaitAsync(sourceToken);
                    if (ShouldClientBeInitialized(deviceStatus))
                    {
                        if (client != null)
                        {
                            await client.DisposeAsync();
                        }

                        client = DeviceClient.CreateFromConnectionString(Config.IotHubConnectionString, TransportType.Mqtt);

                        var retryPolicy = new ExponentialBackoff(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
                        client.SetRetryPolicy(retryPolicy);
                        //client.OperationTimeoutInMilliseconds = (uint)(((Config.Interval * 60) - 30) * 1000);
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
            try
            {
                await client.OpenAsync(sourceToken);
                await ConfigureCallbacks(sourceToken);
            }
            catch (Exception ex) 
            { 
                log.LogError(ex.ToString()); 
            }

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
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    log.LogWarning(message);
                    AzureStatusLed.TurnOff();
                    break;
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Disabled:
                    log.LogWarning(message);
                    // � questo che blocca tutto?
                    //await Initialize();
                    break;
            }
        }

        private async Task ConfigureCallbacks(CancellationToken sourceToken)
        {
            var callbacks = new List<Task>
            {
                client.SetMethodHandlerAsync(DeviceMethods.ReadNow, RefreshThermoData, null, sourceToken),
                client.SetMethodHandlerAsync(DeviceMethods.SetManualSetpoint, OverrideSetpoint, null, sourceToken),
                client.SetMethodHandlerAsync(DeviceMethods.ClearManualSetPoint, ClearSetpoint, null, sourceToken),
                client.SetMethodHandlerAsync(DeviceMethods.SetAwayOn, SetAwayOn, null, sourceToken)
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
                input.Setpoint,
                input.UntilEpoch);
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

        public async Task TrySendMessage(APIMessage message, CancellationToken hostedThermoToken)
        {
            var currentOpCts = CancellationTokenSource.CreateLinkedTokenSource(hostedThermoToken);

            if (ShouldClientBeInitialized(deviceStatus))
            {
                await Initialize(currentOpCts.Token);
            }

            if (IsDeviceConnected)
            {
                var blink = AzureStatusLed.Blink(0.1m, 0.1m, currentOpCts.Token);

                while (skippedMessages.TryDequeue(out var enquequedMEssage))
                {
                    bool status = await SendMessage(enquequedMEssage, currentOpCts.Token);
                    if (!status)
                    {
                        // se qualcosa non va, evitiamo di mettere la cera / togliere la cera per sempre
                        break;
                    }
                }

                await SendMessage(message, currentOpCts.Token);

                currentOpCts.Cancel();
                await blink;
            }
            else
            {
                EnqueueMessage(message);
            }

            currentOpCts.Dispose();
        }

        private async Task<bool> SendMessage(APIMessage message, CancellationToken token)
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

            try
            {
                await client.SendEventAsync(iotMessage, token);
                log.LogInformation($"{messageString}");
            }
            catch (OperationCanceledException canc)
            {
                //this only runs if the process is cancelled from the main loop.
                log.LogError("Operation cancelled", canc);
                EnqueueMessage(message, messageString);
                return false;
            }
            catch (Exception ex)
            {
                log.LogError($"Generic exception", ex.ToString());
                EnqueueMessage(message, messageString);
                return false;
            }
            return true;
        }

        private void EnqueueMessage(APIMessage message, string messageString = null)
        {
            skippedMessages.Enqueue(message);
            log.LogWarning($"Enqueued: {messageString ?? JsonConvert.SerializeObject(message)}");
        }

        private bool ShouldClientBeInitialized(ConnectionStatus connectionStatus)
        {
            return (connectionStatus == ConnectionStatus.Disconnected || connectionStatus == ConnectionStatus.Disabled);
        }
    }
}