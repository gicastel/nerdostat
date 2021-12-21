using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Nerdostat.Shared;
using Newtonsoft.Json.Linq;

namespace API.Services
{
    public static class IoTHub
    {
        private const string deviceID = "nerdostatSym";

        private static Lazy<ServiceClient> _client = new Lazy<ServiceClient>(() =>
        {
            return ServiceClient.CreateFromConnectionString(System.Environment.GetEnvironmentVariable("NerdostatC2D"));
        });

        public static ServiceClient Client => _client.Value;

        public static async Task<CloudToDeviceMethodResult> ReadNow()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.ReadNow) { ResponseTimeout = TimeSpan.FromSeconds(30) };
            methodInvocation.SetPayloadJson("10");

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> AwayOn()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetAwayOn) { ResponseTimeout = TimeSpan.FromSeconds(30) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
        public static async Task<CloudToDeviceMethodResult> AwayOff()
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetAwayOff) { ResponseTimeout = TimeSpan.FromSeconds(30) };
   
            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }

        public static async Task<CloudToDeviceMethodResult> SetManualSetpoint(double setpoint, float? hours)
        {
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.SetManualSetpoint) { ResponseTimeout = TimeSpan.FromSeconds(30) };
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
            var methodInvocation = new CloudToDeviceMethod(DeviceMethods.ClearManualSetPoint) { ResponseTimeout = TimeSpan.FromSeconds(30) };

            // Invoke the direct method asynchronously and get the response from the simulated device.
            return await Client.InvokeDeviceMethodAsync(deviceID, methodInvocation);
        }
    }
}
