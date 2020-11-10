using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using API.Services;
using System.Net.Http;
using Nerdostat.Shared;

namespace API
{
    public static class ManualSetpoint
    {
        [FunctionName("ManualSetpoint")]
        public static async Task<IActionResult> AddSetpoint(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "setpoint/add")] HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                var request = await req.Content.ReadAsStringAsync();
                var requestData = JsonConvert.DeserializeObject<SetPointMessage>(request);
                
                return new OkObjectResult(await IoTHub.SetManualSetpoint(requestData.Setpoint, requestData.Hours));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName("ClearManualSetpoint")]
        public static async Task<IActionResult> RemoveSetpoint(
           [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "setpoint/clear")] HttpRequestMessage req,
           ILogger log)
        {
            try
            {
                return new OkObjectResult(await IoTHub.ClearManualSetpoint());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
