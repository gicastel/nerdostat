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

        private DeviceClient client;

        private volatile bool IsConnected;

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
                await deviceSemaphore.WaitAsync().ConfigureAwait(false);
                IsConnected = false;
                if (client != null)
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }

                var options = new ClientOptions
                {
                    SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset,
                };

                client = DeviceClient.CreateFromConnectionString(Config.IotHubConnectionString, TransportType.Mqtt, options);
                
                var retryPolicy = new ExponentialBackoff(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
                client.SetRetryPolicy(retryPolicy);
                client.OperationTimeoutInMilliseconds = (uint)(((Config.Interval * 60) - 30) * 1000);
                //await client.OpenAsync();
                client.SetConnectionStatusChangesHandler(async (status, reason) => ConnectionChanged(status, reason));
                // need this to Open the connection
                await ConfigureCallbacks().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
            finally
            {
                deviceSemaphore.Release();
            }
        }

        private async void ConnectionChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            string message = $"Connection Status: {status} : {reason}";
            switch (status)
            {
                case ConnectionStatus.Connected:
                    log.LogInformation(message);
                    AzureStatusLed.TurnOn();
                    IsConnected = true;

                    while (skippedMessages.TryDequeue(out var apiMessage))
                    {
                        await SendIotMessage(apiMessage, null).ConfigureAwait(false);
                    }
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    log.LogWarning(message);
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    break;
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Disabled:
                    log.LogWarning(message);
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    await deviceSemaphore.WaitAsync().ConfigureAwait(false);
                    client.Dispose();
                    client = null;
                    deviceSemaphore.Release();
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

            await Task.WhenAll(callbacks).ConfigureAwait(false);

        }

        private async Task<MethodResponse> RefreshThermoData(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"{DeviceMethods.ReadNow}");
            var thermoData = await Thermo.Refresh().ConfigureAwait(false);
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
            return await RefreshThermoData(methodRequest, userContext).ConfigureAwait(false);
        }

        private async Task<MethodResponse> ClearSetpoint(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"WEBR: {DeviceMethods.ClearManualSetPoint}");

            Thermo.ReturnToProgram();
            return await RefreshThermoData(methodRequest, userContext).ConfigureAwait(false);
        }

        private async Task<MethodResponse> SetAwayOn(MethodRequest methodRequest, object userContext)
        {
            log.LogInformation($"WEBR: {DeviceMethods.SetAwayOn}");

            Thermo.SetAway();
            return await RefreshThermoData(methodRequest, userContext).ConfigureAwait(false);
        }

        public async Task SendIotMessage(APIMessage message, CancellationToken? hubOperationToken)
        {
            var messageString = JsonConvert.SerializeObject(message);
            if (client is null)
            {
                if (deviceSemaphore.CurrentCount == 1)
                    await Initialize().ConfigureAwait(false);
                else
                {
                    //in fase di inizializzazione
                    EnqueueMessage(message, messageString);
                }
            }


            CancellationTokenSource currentOpCts;
            if (hubOperationToken.HasValue)
                currentOpCts = CancellationTokenSource.CreateLinkedTokenSource(hubOperationToken.Value);
            else
            {
                currentOpCts = new CancellationTokenSource();
            }
            //currentOpCts.CancelAfter(((Config.Interval * 60) - 30) * 1000);

            if (IsConnected)
            {
                var messageBytes = Encoding.UTF8.GetBytes(messageString);
                var iotMessage = new Message(messageBytes)
                {
                    ContentEncoding = "utf-8",
                    ContentType = "application/json"
                };

                if (Config.TestDevice)
                    iotMessage.Properties.Add("testDevice", "true");

                var blink = AzureStatusLed.Blink((decimal)0.1, (decimal)0.1, currentOpCts.Token).ConfigureAwait(false);
                try
                {
                    await client.SendEventAsync(iotMessage, currentOpCts.Token).ConfigureAwait(false);
                    currentOpCts.Cancel();
                    await blink;
                    //AzureStatusLed.TurnOn();
                    //IsConnected = true;
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
                    //AzureStatusLed.TurnOff();
                    //IsConnected = false;
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
    }
}