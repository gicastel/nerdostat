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
            var options = new ClientOptions
            {
                SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset,
            };
            client ??= DeviceClient.CreateFromConnectionString(ConnectionString, ttype, options);
            var retryPolicy = new ExponentialBackoff(int.MaxValue, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(5));
            client.SetRetryPolicy(retryPolicy);
            client.SetConnectionStatusChangesHandler((status, reason) => ConnectionChanged(status, reason));
            IsTestDevice = _testDevice ?? false;
            this.thermo = _thermo;
            AzureStatusLed = new OuputPin(AzureStatusPinNumber);
        }

        private void ConnectionChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            switch(status)
            {
                case ConnectionStatus.Connected:
                    Trace.TraceInformation($"{status} : {reason}");
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    Trace.TraceWarning($"{status} : {reason}");
                    break;
                case ConnectionStatus.Disconnected:
                    Trace.TraceWarning($"{status} : {reason}");
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
           Trace.TraceInformation(DeviceMethods.ReadNow);
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
            Trace.TraceInformation(stringData);
            var byteData = Encoding.UTF8.GetBytes(stringData);
            return new MethodResponse(byteData, 200);
        }

        private async Task<MethodResponse> OverrideSetpoint(MethodRequest methodRequest, object userContext)
        {
            Trace.TraceInformation(DeviceMethods.SetManualSetpoint);

            var input = JsonConvert.DeserializeObject<SetPointMessage>(methodRequest.DataAsJson);
            thermo.OverrideSetpoint(
                Convert.ToDecimal(input.Setpoint),
                Convert.ToInt32(input.Hours));
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> ClearSetpoint(MethodRequest methodRequest, object userContext)
        {
            Trace.TraceInformation(DeviceMethods.ClearManualSetPoint);

            thermo.ReturnToProgram();
            return await RefreshThermoData(methodRequest, userContext);
        }

        private async Task<MethodResponse> SetAwayOn(MethodRequest methodRequest, object userContext)
        {
            Trace.TraceInformation(DeviceMethods.SetAwayOn);

            thermo.SetAway();
            return await RefreshThermoData(methodRequest, userContext);
        }

        public async Task SendIotMessage(APIMessage message)
        {
            var cts = new CancellationTokenSource();
            var blink = AzureStatusLed.Blink((decimal)0.1, (decimal)0.1, cts.Token);
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            var iotMessage = new Message(messageBytes)
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json"
            };

            if (IsTestDevice)
                iotMessage.Properties.Add("testDevice", "true");

            try
            {
                Trace.TraceInformation(JsonConvert.SerializeObject(message));
                await client.SendEventAsync(iotMessage);
                cts.Cancel();
                await blink;
                AzureStatusLed.TurnOn();
            }
            catch (Exception ex)
            {
               Trace.TraceError(ex.ToString());
                cts.Cancel();
                AzureStatusLed.TurnOff();
            }
        }
    }
}