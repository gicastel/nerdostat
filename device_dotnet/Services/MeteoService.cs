using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Nerdostat.Device.Models;

namespace Nerdostat.Device.Services
{
    public class MeteoService
    {
        const float latitude = 44.1613F;
        const float longitude = 10.8941F;
        const string timezone = "Europe/Berlin";
        private readonly HttpClient _httpClient;
        private readonly SqliteDatastore _datastore;


        public MeteoService(HttpClient httpClient, SqliteDatastore sqlDataStore)
        {
            _httpClient = httpClient;
            _datastore = sqlDataStore;

            Timer requestData = new Timer(async (e) => await GetMeteoDataAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(1));

            // check if we have any missing data
            var pastDays = _datastore.GetMissingMeteoDataDays();
            GetMeteoDataAsync(2, pastDays).GetAwaiter().GetResult();
        }

        public async Task GetMeteoDataAsync(int forecastDays = 2, int pastDays = 1)
        {
            // https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41
            // &current=temperature_2m,relative_humidity_2m,precipitation,cloud_cover
            // &hourly=temperature_2m,relative_humidity_2m,precipitation,cloud_cover
            // &timezone=Europe%2FBerlin
            // &past_days=1&forecast_days=1

            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,precipitation,cloud_cover&hourly=temperature_2m,relative_humidity_2m,precipitation,cloud_cover&timezone={timezone}&past_days={pastDays}&forecast_days={forecastDays}";

            var meteoData = await _httpClient.GetFromJsonAsync<MeteoDataResponse>(url);
            
            for (int i = 0; i < meteoData.Hourly.Time.Length; i++)
            {
                _datastore.UpsertMeteoData(meteoData.Hourly.Time[i], meteoData.Hourly.Temperature2M[i], meteoData.Hourly.RelativeHumidity2M[i], meteoData.Hourly.Precipitation[i], meteoData.Hourly.CloudCover[i]);
            }
        }
    }
}