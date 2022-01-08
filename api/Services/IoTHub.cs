using Microsoft.Azure.Devices;
using Nerdostat.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public static class IoTHub
    {
        private static string deviceID => System.Environment.GetEnvironmentVariable("NerdostatDeviceId");

        private const int Timeout = 15;

        private static Lazy<ServiceClient> _client = new Lazy<ServiceClient>(() =>
        {
            return ServiceClient.CreateFromConnectionString(System.Environment.GetEnvironmentVariable("NerdostatC2D"));
        });

        public static ServiceClient Client => _client.Value;

        public static async Task<CloudToDeviceMethodResult> ReadNow()
        {
            var methodInvocation = new CloudToDeviceMethod("ReadNow") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> AwayOn()
        {
            var methodInvocation = new CloudToDeviceMethod("SetAwayOn") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
        public static async Task<CloudToDeviceMethodResult> AwayOff()
        {
            var methodInvocation = new CloudToDeviceMethod("SetAwayOff") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
   
            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> SetManualSetpoint(double setpoint, float? hours)
        {
            var methodInvocation = new CloudToDeviceMethod("SetManualSetPoint") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
            JObject payload = new JObject();
            payload.Add("setpoint", setpoint);
            if (hours.HasValue)
                payload.Add("hours", hours.Value);
            methodInvocation.SetPayloadJson(payload.ToString());

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> ClearManualSetpoint()
        {
            var methodInvocation = new CloudToDeviceMethod("ClearManualSetPoint") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> GetProgram()
        {
            var methodInvocation = new CloudToDeviceMethod("GetProgram") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> SetProgram(ProgramMessage programMessage)
        {
            var methodInvocation = new CloudToDeviceMethod("SetProgram") { ResponseTimeout = TimeSpan.FromSeconds(Timeout) };
            //var payload = new { program = programMessage };
            JObject payload = new JObject(
                new JProperty("program", JObject.FromObject(programMessage))
                );

            methodInvocation.SetPayloadJson(payload.ToString());

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
    }
}
