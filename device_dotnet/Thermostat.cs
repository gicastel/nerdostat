using System;
using System.Device.Gpio;
using System.Threading.Tasks;
using Iot.Device.DHTxx;
using Nerdostat.Shared;

namespace Nerdostat.Device
{
    public class Thermostat
    {
        private int HeaterRelayPinNumber = 21;
        private int StatusLedPinNumber = 17;
        private int DhtPinNumber = 4;

        private OuputPin HeaterRelay;
        private OuputPin StatusLed;

        private Configuration config { get; set; }

      
        public Thermostat(Configuration _config)
        {
            config = _config;
            
            HeaterRelay = new OuputPin(HeaterRelayPinNumber);
            StatusLed = new OuputPin(StatusLedPinNumber);
        }
        

        public async Task<APIMessage> Refresh()
        {
            (double temperature, double relativeHumidity) = await ReadValues();
            var setpoint = GetCurrentSetpoint();

            var diff = Convert.ToDecimal(temperature) - setpoint;
            
            if (Math.Abs(diff) > config.Threshold)
            {
                if (diff < 0)
                    StartHeater();
                else
                    StopHeater();
            }

            var heaterTime = GetHeaterTime();
            var heaterIsActive = HeaterRelay.IsOn();

            int overrideSecondsRemaining = 0;
            if (config.OverrideUntil.HasValue)
                overrideSecondsRemaining = Convert.ToInt32((DateTime.Now - config.OverrideUntil.Value).TotalSeconds);

            var save = config.SaveConfiguration();

            var msg = new APIMessage(){
                CurrentSetpoint = (double)setpoint,
                HeaterOn = heaterTime,
                Humidity = relativeHumidity,
                Temperature = temperature,
                Timestamp = DateTime.Now,
                IsHeaterOn = heaterIsActive
            };

            return msg;
        }

        public void OverrideSetpoint(decimal setpoint, int? hours)
        {
            config.OverrideSetpoint = setpoint;
            var setpointuntil = DateTime.Now.AddHours(Convert.ToDouble(hours.HasValue ? hours : config.OverrideDefaultDuration));
            config.OverrideUntil = setpointuntil;
        }

        public void ReturnToProgram()
        {
            config.OverrideSetpoint = null;
            config.OverrideUntil = DateTime.Now.AddSeconds(-10);
        }

        public void SetAway()
        {
            config.OverrideSetpoint = config.AwaySetpoint;
            config.OverrideUntil = DateTime.Now.AddYears(1);
        }

        #region Privates


        private bool IsSetpointOverridden()
        {
            if (config.OverrideUntil.HasValue)
            {
                if (config.OverrideUntil.Value >= DateTime.Now)
                    return true;
            }

            return false;
        }

        private decimal GetCurrentSetpoint()
        {
            if (IsSetpointOverridden())
                return config.OverrideSetpoint.Value;
            else
            {
                // program [monday] [08] [25 / 15 = 1]
                int dow = (int)DateTime.Now.DayOfWeek;
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                minute -= minute % 15;
                return config.Program[dow][hour][minute];
            }
        }

        #endregion

    
        #region Hardware

        private async Task<(double temperature, double relativeHumidity)> ReadValues()
        {
#if DEBUG
            return await GenerateValues();
#endif
            using var controller = new GpioController();
            var sensor = new Dht22(DhtPinNumber, PinNumberingScheme.Board, controller);
            var temp = sensor.Temperature;
            var hum = sensor.Humidity;
            while(!sensor.IsLastReadSuccessful)
            {
                await Task.Delay(1000);
                temp = sensor.Temperature;
                hum = sensor.Humidity;
            }
            return (temp.DegreesCelsius, hum.Percent);
        }

        private async Task<(double temperature, double relativeHumidity)> GenerateValues()
        {
            return (Random.Shared.Next(15, 25), Random.Shared.Next(30, 90));
        }

        private void StartHeater()
        {

            if (!config.HeaterOnSince.HasValue)
                config.HeaterOnSince = DateTime.Now;
        }

        private void StopHeater()
        {
            HeaterRelay.TurnOff();
            StatusLed.TurnOff();
        }

        private int GetHeaterTime()
        {
            int seconds = 0;
            if (config.HeaterOnSince.HasValue)
            {
                var ts = DateTime.Now;
                var delta = ts - config.HeaterOnSince.Value;
                config.HeaterOnSince =  (HeaterRelay.IsOn()? ts : null);
                seconds = Convert.ToInt32(delta.TotalSeconds);
            }
            return seconds;
        }

        #endregion

    }
}
