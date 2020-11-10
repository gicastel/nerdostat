using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using API.Services;

namespace API
{
    public static class Away
    {
        [FunctionName("SetAwayON")]
        public static async Task<IActionResult> GetAway(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "away/on")] HttpRequest req,
            ILogger log)
        {
            try
            {
                return new OkObjectResult(await IoTHub.AwayOn());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName("SetAwayOFF")]
        public static async Task<IActionResult> SetAway(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "away/off")] HttpRequest req,
            ILogger log)
        {
            try
            {
                return new OkObjectResult(await IoTHub.AwayOff());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
