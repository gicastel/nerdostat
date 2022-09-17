using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Nerdostat.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device
{
    public class Hub
    {
        private int AzureStatusPinNumber = 18;

        private OuputPin AzureStatusLed;

        private readonly Thermostat thermo;

        private DeviceClient client;

        private readonly string ConnectionString;

        private bool IsTestDevice;

        private bool IsConnected;

        TransportType ttype = TransportType.Mqtt;
        public static async Task<Hub> Initialize(string connectionString, bool? testDevice, Thermostat thermo)
        {
            var hub = new Hub(connectionString, testDevice, thermo);
            await hub.ConfigureCallbacks();

            return hub;
        }

        private Hub(string connectionString, bool? _testDevice, Thermostat _thermo)
        {
            ConnectionString = connectionString;
            IsConnected = false;
            var options = new ClientOptions
            {
                SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset,
            };
            client ??= DeviceClient.CreateFromConnectionString(ConnectionString, ttype, options);
            var retryPolicy = new ExponentialBackoff(int.MaxValue, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
            client.SetRetryPolicy(retryPolicy);
            client.SetConnectionStatusChangesHandler((status, reason) => ConnectionChanged(status, reason));
            client.OperationTimeoutInMilliseconds = 5 * 60 * 1000;
            IsTestDevice = _testDevice ?? false;
            this.thermo = _thermo;
            AzureStatusLed = new OuputPin(AzureStatusPinNumber);
        }

        private void ConnectionChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            switch(status)
            {
                case ConnectionStatus.Connected:
                    Console.WriteLine($"INFO: {status} : {reason}");
                    AzureStatusLed.TurnOn();
                    IsConnected = true;
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    Console.WriteLine($"WARN: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    break;
                case ConnectionStatus.Disconnected:
                    Console.WriteLine($"WARN: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                    break;
                case ConnectionStatus.Disabled:
                    Console.WriteLine($"WARN: {status} : {reason}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
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
            Console.WriteLine($"WEBR: {DeviceMethods.ReadNow}");
            var thermoData = await thermo.Refresh();
            var message = new APIMessage(){
                Timestamp = DateTime.Now,
                Temperature = thermoData.Temperature,
                Humidity = thermoData.Humidity,
                CurrentSetpoint = thermoData.CurrentSetpoint,
                HeaterOn = Convert.ToInt64(thermoData.HeaterOn),
                IsHeaterOn = thermoData.IsHeaterOn,
                OverrideEnd = thermoData.OverrideEnd
            };

            var stringData = JsonConvert.SerializeObject(message);
            Console.WriteLine("WEBR: Reply " + stringData);
            var byteData = Encoding.UTF8.GetBytes(stringData);
            return new MethodResponse(byteData, 200);
        }

        private async Task<MethodResponse> OverrideSetpoint(MethodRequest methodRequest, object userContext)
        {

            Console.WriteLine($"WEBR: {DeviceMethods.SetManualSetpoint}");
            var input = JsonConvert.DeserializeObject<SetPointMessage>(methodRequest.DataAsJson);
            thermo.OverrideSetpoint(
                Convert.ToDecimal(input.Setpoint),
                Convert.ToInt32(input.Hours));
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> ClearSetpoint(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"WEBR: {DeviceMethods.ClearManualSetPoint}");

            thermo.ReturnToProgram();
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> SetAwayOn(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"WEBR: {DeviceMethods.SetAwayOn}");

            thermo.SetAway();
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

            if (IsTestDevice)
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
                    Console.WriteLine($"INFO: {messageString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRR: {ex}");
                    AzureStatusLed.TurnOff();
                    IsConnected = false;
                }
            }
            else
            {
                Console.WriteLine($"WARN: Skipped message");
            }
        }
    }
}