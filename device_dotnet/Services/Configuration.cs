using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Nerdostat.Device.Services
{
    public class Configuration
    {

        // Monday => 08 => 15 => 20.5 °C
        public Dictionary<int, Dictionary<int, Dictionary<int, decimal>>> Program { get; set; }
        public decimal Threshold { get; set; }

        [JsonProperty]
        private long? OverrideUntilEpoch { get; set; }
        public DateTime? OverrideUntil
        {
            get
            {
                if (OverrideUntilEpoch.HasValue)
                    return new DateTime(1970, 1, 1).AddSeconds(OverrideUntilEpoch.Value);
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                    OverrideUntilEpoch = (long)(value.Value - new DateTime(1970, 1, 1)).TotalSeconds;
                else
                    OverrideUntilEpoch = null;
            }
        }
        public decimal? OverrideSetpoint { get; set; }
        public int OverrideDefaultDuration { get; set; }
        [JsonProperty]
        private long? HeaterOnSinceEpoch { get; set; }
        public DateTime? HeaterOnSince
        {
            get
            {
                if (HeaterOnSinceEpoch.HasValue)
                    return new DateTime(1970, 1, 1).AddSeconds(HeaterOnSinceEpoch.Value);
                else
                    return null;
            }
            set
            {
                if (value.HasValue)
                    HeaterOnSinceEpoch = (long)(value.Value - new DateTime(1970, 1, 1)).TotalSeconds;
                else
                    HeaterOnSinceEpoch = null;
            }
        }

        public decimal AwaySetpoint { get; set; }
        public decimal NoFrostSetpoint { get; set; }
        public string IotHubConnectionString { get; set; }
        public bool TestDevice { get; set; }

        private const string configFilePath = "config.json";

        public Configuration()
        {

        }

        public async Task LoadConfiguration(bool regenConfig)
        {
            FileInfo cfgFile = new FileInfo(configFilePath);
            if (!cfgFile.Exists || regenConfig)
            {
                Program = CreateDefaultProgram();

                Threshold = (decimal)0.2;
                OverrideDefaultDuration = 4;
                AwaySetpoint = 12;
                NoFrostSetpoint = 5;
                string content = JsonConvert.SerializeObject(this);

                using (StreamWriter wr = new StreamWriter(configFilePath, false))
                {
                    await wr.WriteAsync(content);
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(configFilePath))
                {
                    string content = await sr.ReadToEndAsync();

                    Configuration loaded = JsonConvert.DeserializeObject<Configuration>(content);

                    this.Program = loaded.Program;
                    this.Threshold = loaded.Threshold;
                    this.NoFrostSetpoint = loaded.NoFrostSetpoint;
                    this.OverrideDefaultDuration = loaded.OverrideDefaultDuration;
                    this.OverrideSetpoint = loaded.OverrideSetpoint;
                    this.AwaySetpoint = loaded.AwaySetpoint;
                    this.HeaterOnSince = loaded.HeaterOnSince;
                    this.IotHubConnectionString = loaded.IotHubConnectionString;
                    this.OverrideUntil = loaded.OverrideUntil;
                    this.TestDevice = loaded.TestDevice;
                }
            }
        }

        public async Task SaveConfiguration()
        {
            FileInfo cfgFile = new FileInfo(configFilePath);
            string content = JsonConvert.SerializeObject(this);
            using (StreamWriter wr = new StreamWriter(configFilePath, false))
            {
                await wr.WriteAsync(content);
            }
        }

        private static Dictionary<int, Dictionary<int, Dictionary<int, decimal>>> CreateDefaultProgram()
        {
            Dictionary<int, Dictionary<int, Dictionary<int, decimal>>> days = new();
            for (int dow = 0; dow < 7; dow++)
            {
                Dictionary<int, Dictionary<int, decimal>> hours = new();
                for (int hour = 0; hour < 24; hour++)
                {
                    Dictionary<int, decimal> quarters = new();
                    for (int q = 0; q < 60; q += 15)
                    {
                        decimal temp;
                        if (8 <= hour && hour <= 22)
                            temp = 20;
                        else
                            temp = 18;

                        quarters.Add(q, temp);
                    }
                    hours.Add(hour, quarters);
                }
                days.Add(dow, hours);
            }

            return days;
        }

    }
}