using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Nerdostat.Shared;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace API
{

    public static class Hub
    {
#if DEBUG
        [Disable]
#endif
        [FunctionName("ReadMessage")]
        public static async Task Run([EventHubTrigger("%EventHubName%", Connection = "NerdostatD2C")] EventData message, ILogger log)
        {
            IotMessage msg = JsonConvert.DeserializeObject<IotMessage>(Encoding.UTF8.GetString(message.EventBody));

            if (message.Properties.ContainsKey("testDevice"))
            {
                log.LogInformation(Encoding.UTF8.GetString(message.EventBody));
                return;
            }

            if (msg.Temperature.HasValue)
                log.LogMetric("Temperature", msg.Temperature.Value);
            if (msg.Humidity.HasValue)
                log.LogMetric("Humidity", msg.Humidity.Value);
            log.LogMetric("Setpoint", msg.CurrentSetpoint);
            log.LogMetric("HeaterOn", msg.HeaterOn);
            if (msg.PredictedTemperature.HasValue)
                log.LogMetric("PredictedTemperature", msg.PredictedTemperature.Value);

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
    }
}