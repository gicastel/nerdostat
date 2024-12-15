using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Newtonsoft.Json;

namespace Nerdostat.Device.Services
{
    public class MeteoService
    {
        const decimal latitude = 44.1613M;
        const decimal longitude = 10.8941M;
        const string timezone = "Europe/Berlin";
        private readonly HttpClient _httpClient;
        private readonly SqliteDatastore _datastore;
        private readonly ILogger<MeteoService> log;

        private Timer requestData;

        public MeteoService(HttpClient httpClient, SqliteDatastore sqlDataStore, ILogger<MeteoService> _log)
        {
            _httpClient = httpClient;
            _datastore = sqlDataStore;
            log = _log;

            requestData = new Timer(async (e) => await GetMeteoDataAsync(), null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        }

        public async Task Initialize()
        {
            var pastDays = _datastore.GetMissingMeteoDataDays();
            if (pastDays < 0)
            {
                log.LogInformation("No missing meteo data");
                return;
            }
            else
            {
                log.LogInformation($"Getting meteo data for {pastDays} days");
                await GetMeteoDataAsync(2, pastDays);
            }
        }

        public async Task GetMeteoDataAsync(int forecastDays = 2, int pastDays = 1)
        {
            // https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41
            // &current=temperature_2m,relative_humidity_2m,precipitation,cloud_cover
            // &hourly=temperature_2m,relative_humidity_2m,precipitation,cloud_cover
            // &timezone=Europe%2FBerlin
            // &past_days=1&forecast_days=1

            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString().Replace(',','.')}&longitude={longitude.ToString().Replace(',', '.')}&current=temperature_2m,relative_humidity_2m,precipitation,cloud_cover&hourly=temperature_2m,relative_humidity_2m,precipitation,cloud_cover&timezone={timezone}&past_days={pastDays}&forecast_days={forecastDays}";

            var meteoDataResponse = await _httpClient.GetStringAsync(url);

            // serialize data
            var meteoData = JsonConvert.DeserializeObject<MeteoDataResponse>(meteoDataResponse);

            for (int i = 0; i < meteoData.Hourly.Time.Length; i++)
            {
                _datastore.UpsertMeteoData(meteoData.Hourly.Time[i], meteoData.Hourly.Temperature2M[i], meteoData.Hourly.RelativeHumidity2M[i], meteoData.Hourly.Precipitation[i], meteoData.Hourly.CloudCover[i]);
            }
        }
    }
}