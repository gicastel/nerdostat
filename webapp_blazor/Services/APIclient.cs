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
    }

    public class APIclient : IAPIClient
    {
        private HttpClient _client;

        private readonly AppConfig config;

        public APIclient(AppConfig _config)
        {
            this.config = _config;

            _client = new HttpClient();
            _client.BaseAddress = new Uri(config.NerdostatAPIEndpoint);
            _client.DefaultRequestHeaders.Add("x-functions-key", config.NerdostatAPIKey);
        }

        public async Task<APIMessage> GetData()
        {
            var response = await _client.GetAsync(_client.BaseAddress + "read");
            var msgString = await response.Content.ReadAsStringAsync();
            var msg = JsonConvert.DeserializeObject<APIResponse>(msgString);
            var payload = JsonConvert.DeserializeObject<APIMessage>(msg.payload);
            return payload;
        }
    }
}
