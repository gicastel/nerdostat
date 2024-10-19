using System;
using System.Collections.Generic;

namespace Nerdostat.Shared
{
    public class IotMessage
    {
        public DateTime Timestamp { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public double CurrentSetpoint { get; set; }
        public double HeaterOn { get; set; }
    }

    public class APIMessage
    {
        public DateTime Timestamp { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public decimal CurrentSetpoint { get; set; }
        public bool IsHeaterOn { get; set; }
        public long? OverrideEnd { get; set; }
        public long? HeaterOn { get; set; }
        public double? PredictedTemperature { get; set; }
    }

    public class APIResponse<T>
    {
        public int status { get; set; }
        public T payload { get; set; }
    }

    public class ProgramMessage : Dictionary<int, Dictionary<int, Dictionary<int, decimal>>>
    {

    }

    public class SetPointMessage
    {
        public decimal Setpoint { get; set; }
        public long? UntilEpoch { get; set; }
    }

    public static class DeviceMethods
    {
        public static string ReadNow => "ReadNow";
        public static string SetManualSetpoint => "SetManualSetPoint";
        public static string ClearManualSetPoint => "ClearManualSetpoint";
        public static string SetAwayOn => "SetAwayOn";
        public static string SetAwayOff => "SetAwayOff";
        public static string GetProgram => "GetProgram";
        public static string SetProgram => "SetProgram";
    }

    public enum PyWeekDays
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}
