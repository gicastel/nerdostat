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
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace API
{
    public static class App
    {
        [FunctionName(nameof(ReadFromApp))]
        public static async Task<IActionResult> ReadFromApp(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "read")] HttpRequestMessage req,
          ILogger log)
        {
            try
            {
                return new OkObjectResult(await IoTHub.ReadNow());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName(nameof(AddSetpoint))]
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

        [FunctionName(nameof(RemoveSetpoint))]
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

        [FunctionName(nameof(SetAwayOn))]
        public static async Task<IActionResult> SetAwayOn(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "away/on")] HttpRequest req,
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

        [FunctionName(nameof(SetAwayOff))]
        public static async Task<IActionResult> SetAwayOff(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "away/off")] HttpRequest req,
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

        [FunctionName(nameof(GetProgram))]
        public static async Task<IActionResult> GetProgram(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "program")] HttpRequest req,
    ILogger log)
        {
            try
            {
                return new OkObjectResult(await IoTHub.GetProgram());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName(nameof(SetProgram))]
        public static async Task<IActionResult> SetProgram(
  [HttpTrigger(AuthorizationLevel.Function, "post", Route = "program")] HttpRequestMessage req,
  ILogger log)
        {
            try
            {
                var request = await req.Content.ReadAsStringAsync();
                var requestData = JsonConvert.DeserializeObject<ProgramMessage>(request);

                return new OkObjectResult(await IoTHub.SetProgram(requestData));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
