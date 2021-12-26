using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Nerdostat.Device
{
    public class Configuration
    {

        // Monday => 08 => 15 => 20.5 °C
        public Dictionary<int, Dictionary <int, Dictionary<int, decimal>>> Program { get; set; }
        public decimal Threshold { get; set; }

        [JsonProperty]
        private long? OverrideUntilEpoch { get; set; }
        public DateTime? OverrideUntil { 
        get        
            {
                if(OverrideUntilEpoch.HasValue)
                    return new DateTime(1970, 1, 1).AddSeconds(OverrideUntilEpoch.Value);
                else
                    return null;
            }
        set 
            {
                if (value.HasValue)
                    this.OverrideUntilEpoch = (long)(value.Value - new DateTime(1970, 1, 1)).TotalSeconds;
                else 
                    this.OverrideUntilEpoch = null;
            }
        }
        public decimal? OverrideSetpoint { get; set; }
        public int OverrideDefaultDuration { get; set; }

        private long? HeaterOnSinceEpoch { get; set; }
        public DateTime? HeaterOnSince 
        { 
        get        
            {
                if(HeaterOnSinceEpoch.HasValue)
                    return new DateTime(1970, 1, 1).AddSeconds(HeaterOnSinceEpoch.Value);
                else
                    return null;
            }
        set 
            {
                if (value.HasValue)
                    this.HeaterOnSinceEpoch = (long)(value.Value - new DateTime(1970, 1, 1)).TotalSeconds;
                else 
                    this.HeaterOnSinceEpoch = null;
            }
        } 
        
        public decimal AwaySetpoint { get; set; }
        public decimal NoFrostSetpoint { get; set; }
        public string IotHubConnectionString { get;set; }
        public bool TestDevice { get; set; }

        private const string configFilePath = "config.json";

        private Configuration()
        {
            
        }

        public static async Task<Configuration> LoadConfiguration(bool regenConfig)
        {
            FileInfo cfgFile = new FileInfo(configFilePath);
            if (!cfgFile.Exists || regenConfig)
            {
                Configuration dflt = new();
                dflt.Program = CreateDefaultProgram();

                dflt.Threshold = (decimal)0.2;
                dflt.OverrideDefaultDuration = 4;
                dflt.AwaySetpoint = 12;
                dflt.NoFrostSetpoint = 5;

                string content = JsonConvert.SerializeObject(dflt);

                using(StreamWriter wr = new StreamWriter(configFilePath, false))
                {
                    await wr.WriteAsync(content);
                }

                return dflt;
            }
            else
            {
                using(StreamReader sr = new StreamReader(configFilePath))
                {
                    string content = await sr.ReadToEndAsync();

                    Configuration loaded = JsonConvert.DeserializeObject<Configuration>(content);

                    return loaded;
                }
            }
        }

         public async Task SaveConfiguration()
        {
            FileInfo cfgFile = new FileInfo(configFilePath);
            string content = JsonConvert.SerializeObject(this);
            using(StreamWriter wr = new StreamWriter(configFilePath, false))
            {
                await wr.WriteAsync(content);
            }
        }

        private static Dictionary<int, Dictionary<int , Dictionary<int, decimal>>> CreateDefaultProgram()
        {
            Dictionary<int, Dictionary<int , Dictionary<int, decimal>>> days = new();
            for (int dow = 0; dow < 7; dow++)
            {
                Dictionary<int, Dictionary<int, decimal>> hours = new();
                for (int hour = 0; hour < 24; hour ++)
                {
                    Dictionary<int, decimal> quarters = new();
                    for (int q = 0; q < 60; q+=15)
                    {
                        Decimal temp;
                        if ( 8 <= hour && hour <= 22)
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