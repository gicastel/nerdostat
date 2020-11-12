using BlazorClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nerdostat.Shared;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace BlazorClient.Services
{
    public interface IAPIClient
    {
        Task<APIMessage> GetData();
        Task<APIMessage> ModifySetPoint(double newTempValue, int? duration);
        Task<APIMessage> ResetSetPoint();

    }

    public class APIClient : IAPIClient
    {
        private HttpClient _client;

        private readonly AppConfig config;

        public APIClient(AppConfig _config)
        {
            this.config = _config;

            _client = new HttpClient();
            _client.BaseAddress = new Uri(config.NerdostatAPIEndpoint);
            _client.DefaultRequestHeaders.Add("x-functions-key", config.NerdostatAPIKey);
        }

        public async Task<APIMessage> GetData()
        {
            var response = await _client.GetAsync(_client.BaseAddress + "read");
            var msg = await response.Content.ReadFromJsonAsync<APIResponse>();
            var payload = JsonConvert.DeserializeObject<APIMessage>(msg.payload);
            return payload;
        }

        public async Task<APIMessage> ModifySetPoint(double newTempValue, int? duration)
        {
            var setpoint = new SetPoint(newTempValue, duration ?? 4);
            var response = await _client.PostAsJsonAsync<SetPoint>(_client.BaseAddress + "setpoint/add", setpoint);
            var msg = await response.Content.ReadFromJsonAsync<APIResponse>();
            var payload = JsonConvert.DeserializeObject<APIMessage>(msg.payload);
            return payload;
        }

        public async Task<APIMessage> ResetSetPoint()
        {
            var response = await _client.PostAsync(_client.BaseAddress + "setpoint/clear", null);
            var msg = await response.Content.ReadFromJsonAsync<APIResponse>();
            var payload = JsonConvert.DeserializeObject<APIMessage>(msg.payload);
            return payload;
        }

        private record SetPoint(double setpoint, int hours);
    }
}
