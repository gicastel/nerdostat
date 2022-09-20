using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Nerdostat.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Services
{
    public class Hub
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

        public Hub(Configuration _config, Thermostat _thermo, ILogger<Hub> _log)
        {
            Config = _config;
            Thermo = _thermo;
            log = _log;
            
            AzureStatusLed = new(AzureStatusPinNumber, log);
        }
        public async Task Initialize()
        {
            await deviceSemaphore.WaitAsync();
            IsConnected = false;
            if (client != null)
            {
                await client.DisposeAsync();
            }

            var options = new ClientOptions
            {
                SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset,
            };
            client = DeviceClient.CreateFromConnectionString(Config.IotHubConnectionString, TransportType.Mqtt, options);
            var retryPolicy = new ExponentialBackoff(int.MaxValue, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
            client.SetRetryPolicy(retryPolicy);
            client.SetConnectionStatusChangesHandler((status, reason) => ConnectionChanged(status, reason));
            client.OperationTimeoutInMilliseconds = 5 * 60 * 1000;
            await client.OpenAsync();
            await ConfigureCallbacks();
            deviceSemaphore.Release();
        }

        private async void ConnectionChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            switch (status)
            {
                case ConnectionStatus.Connected:
                    log.LogInformation($"Connetion Status: {status} : {reason}");
                    AzureStatusLed.TurnOn();
                    IsConnected = true;
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    log.LogWarning($"Connetion Status: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    break;
                case ConnectionStatus.Disconnected:
                    log.LogWarning($"Connetion Status: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    await Initialize();
                    break;
                case ConnectionStatus.Disabled:
                    log.LogError($"Connetion Status: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    await Initialize();
                    break;
            }
        }

        private async Task ConfigureCallbacks()
        {
            await client.SetMethodHandlerAsync(
                DeviceMethods.ReadNow,
                RefreshThermoData,
                null
            );

            await client.SetMethodHandlerAsync(
                DeviceMethods.SetManualSetpoint,
                OverrideSetpoint,
                null
            );

            await client.SetMethodHandlerAsync(
                DeviceMethods.ClearManualSetPoint,
                ClearSetpoint,
                null
            );

            await client.SetMethodHandlerAsync(
                DeviceMethods.SetAwayOn,
                SetAwayOn,
                null
            );

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

        public async Task SendIotMessage(APIMessage message)
        {
            using var cts = new CancellationTokenSource();
            var blink = AzureStatusLed.Blink((decimal)0.1, (decimal)0.1, cts.Token);
            var messageString = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);
            var iotMessage = new Message(messageBytes)
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json"
            };

            if (Config.TestDevice)
                iotMessage.Properties.Add("testDevice", "true");

            if (IsConnected)
            {
                try
                {
                    await client.SendEventAsync(iotMessage);
                    cts.Cancel();
                    await blink;
                    AzureStatusLed.TurnOn();
                    IsConnected = true;
                    log.LogInformation($"{messageString}");
                }
                catch (Exception ex)
                {
                    log.LogError($"{ex}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                }
            }
            else
            {
                log.LogWarning($"Skipped message: {messageString}");
            }
        }
    }
}