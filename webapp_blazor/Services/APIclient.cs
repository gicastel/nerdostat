using Nerdostat.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorClient.Services
{
    public interface IAPIClient
    {
        Task<APIMessage> GetData();
        Task<APIMessage> ModifySetPoint(decimal newTempValue, long? remainingMinutes);
        Task<APIMessage> ResetSetPoint();

        Task<ProgramMessage> GetProgram();
        Task<ProgramMessage> UpdateProgram(ProgramMessage program);

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
            response.EnsureSuccessStatusCode();
            var msg = await response.Content.ReadFromJsonAsync<APIResponse<APIMessage>>();
            return msg.payload;
        }

        public async Task<APIMessage> ModifySetPoint(decimal newTempValue, long? remainingMinutes)
        {
            long? epoch = null;
            if (remainingMinutes is not null)
                epoch = DateTime.Now.AddMinutes((double)remainingMinutes).ToEpoch();

            var setpoint = new SetPointMessage(){
                Setpoint = newTempValue,
                UntilEpoch = epoch
            };
            var response = await _client.PostAsJsonAsync(_client.BaseAddress + "setpoint/add", setpoint);
            response.EnsureSuccessStatusCode();
            var msg = await response.Content.ReadFromJsonAsync<APIResponse<APIMessage>>();
            return msg.payload;
        }

        public async Task<APIMessage> ResetSetPoint()
        {
            var response = await _client.PostAsync(_client.BaseAddress + "setpoint/clear", null);
            response.EnsureSuccessStatusCode();
            var msg = await response.Content.ReadFromJsonAsync<APIResponse<APIMessage>>();
            return msg.payload;
        }

        public async Task<ProgramMessage> GetProgram()
        {
            var response = await _client.GetAsync(_client.BaseAddress + "program");
            response.EnsureSuccessStatusCode();
            var msg = await response.Content.ReadFromJsonAsync<APIResponse<ProgramMessage>>();
            return msg.payload;
        }

        public async Task<ProgramMessage> UpdateProgram(ProgramMessage program)
        {
            var response = await _client.PostAsJsonAsync(_client.BaseAddress + "program", program);
            response.EnsureSuccessStatusCode();
            var msg = await response.Content.ReadFromJsonAsync<APIResponse<ProgramMessage>>();
            return msg.payload;
        }

        private record SetPoint(double setpoint, long? untilEpoch);
    }
}
