using System;

namespace Nerdostat.Shared
{
    public class IotMessage
    {
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double CurrentSetpoint { get; set; }
        public double HeaterOn { get; set; }
    }

    public class APIMessage
    {
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double CurrentSetpoint { get; set; }
        public bool IsHeaterOn { get; set; }
        public long? OverrideEnd { get; set; }
        public long? HeaterOn { get; set; }
    }

    public class APIResponse
    {
        public int status { get; set; }
        public string payload { get; set; }
    }

    public class SetPointMessage
    {
        public double Setpoint { get; set; }
        public float? Hours { get; set; }
    }
}
