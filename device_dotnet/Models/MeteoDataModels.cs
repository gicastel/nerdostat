using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nerdostat.Device.Models
{
    public class MeteoDataResponse
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("generationtime_ms")]
        public double GenerationtimeMs { get; set; }

        [JsonProperty("utc_offset_seconds")]
        public long UtcOffsetSeconds { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("timezone_abbreviation")]
        public string TimezoneAbbreviation { get; set; }

        [JsonProperty("elevation")]
        public long Elevation { get; set; }

        [JsonProperty("current_units")]
        public Units CurrentUnits { get; set; }

        [JsonProperty("current")]
        public Current Current { get; set; }

        [JsonProperty("hourly_units")]
        public Units HourlyUnits { get; set; }

        [JsonProperty("hourly")]
        public Hourly Hourly { get; set; }
    }

    public partial class Current
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("interval")]
        public long Interval { get; set; }

        [JsonProperty("temperature_2m")]
        public float Temperature2M { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public int RelativeHumidity2M { get; set; }

        [JsonProperty("precipitation")]
        public float Precipitation { get; set; }

        [JsonProperty("cloud_cover")]
        public int CloudCover { get; set; }
    }

    public partial class Units
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("interval", NullValueHandling = NullValueHandling.Ignore)]
        public string Interval { get; set; }

        [JsonProperty("temperature_2m")]
        public string Temperature2M { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public string RelativeHumidity2M { get; set; }

        [JsonProperty("precipitation")]
        public string Precipitation { get; set; }

        [JsonProperty("cloud_cover")]
        public string CloudCover { get; set; }
    }

    public partial class Hourly
    {
        [JsonProperty("time")]
        public DateTime[] Time { get; set; }

        [JsonProperty("temperature_2m")]
        public float[] Temperature2M { get; set; }

        [JsonProperty("relative_humidity_2m")]
        public int[] RelativeHumidity2M { get; set; }

        [JsonProperty("precipitation")]
        public float[] Precipitation { get; set; }

        [JsonProperty("cloud_cover")]
        public int[] CloudCover { get; set; }
    }
}
