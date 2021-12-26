using Microsoft.Azure.WebJobs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using API.Services;
using Nerdostat.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using System.Reflection.Metadata;

namespace API
{

    public static class ReadMessage
    {
//#if DEBUG
//        [Disable]
//#endif
        [FunctionName("ReadMessage")]
        public static async Task Run([EventHubTrigger("%EventHubName%", Connection = "NerdostatD2C")] EventData message, ILogger log)
        {
            IotMessage msg = JsonConvert.DeserializeObject<IotMessage>(Encoding.UTF8.GetString(message.EventBody));

            if (message.Properties.ContainsKey("testDevice"))
            {
                log.LogInformation(Encoding.UTF8.GetString(message.EventBody));
                return;
            }

            log.LogMetric("Temperature", msg.Temperature);
            log.LogMetric("Humidity", msg.Humidity);
            log.LogMetric("Setpoint", msg.CurrentSetpoint);
            log.LogMetric("HeaterOn", msg.HeaterOn);


            HttpClient pbi = new HttpClient();

            var pBiEndpoint = System.Environment.GetEnvironmentVariable("PowerBiEndpoint");

            if (!string.IsNullOrWhiteSpace(pBiEndpoint))
            {
                await pbi.PostAsync<List<IotMessage>>(
                    pBiEndpoint,
                    new List<IotMessage>() { msg },
                    new JsonMediaTypeFormatter()
                    );
            }
        }


        [FunctionName("ReadApp")]
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
    }
}