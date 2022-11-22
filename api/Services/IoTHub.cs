using Microsoft.Azure.Devices;
using Nerdostat.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public static class IoTHub
    {
        private static string deviceID => System.Environment.GetEnvironmentVariable("NerdostatDeviceId");

        private const int Timeout = 30;

        private static Lazy<ServiceClient> _client = new Lazy<ServiceClient>(() =>
        {
            return ServiceClient.CreateFromConnectionString(System.Environment.GetEnvironmentVariable("NerdostatC2D"));
        });

        public static ServiceClient Client => _client.Value;

        public static async Task<CloudToDeviceMethodResult> ReadNow()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.ReadNow) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> AwayOn()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetAwayOn) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
        public static async Task<CloudToDeviceMethodResult> AwayOff()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetAwayOff) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
   
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> SetManualSetpoint(double setpoint, long? untilEpoch)
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetManualSetpoint) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
            JObject payload = new JObject();
            payload.Add("setpoint", setpoint);
            if (untilEpoch.HasValue)
                payload.Add("untilEpoch", untilEpoch.Value);
            methodInvocation.SetPayloadJson(payload.ToString());

            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> ClearManualSetpoint()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.ClearManualSetPoint) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> GetProgram()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.GetProgram) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> SetProgram(ProgramMessage programMessage)
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetProgram) { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
            //var payload = new { program = programMessage };
            JObject payload = new JObject(
                new JProperty("program", JObject.FromObject(programMessage))
                );

            methodInvocation.SetPayloadJson(payload.ToString());

            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
    }
}
