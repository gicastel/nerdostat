using Iot.Device.DHTxx;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Models;
using Nerdostat.Shared;
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;

namespace Nerdostat.Device.Services
{
    public class Thermostat
    {
        private const int HeaterRelayPinNumber = 21;
        private const int StatusLedPinNumber = 17;
        private const int DhtPinNumber = 4;

#if DEBUG
        private readonly MockPin HeaterRelay;
        private readonly MockPin StatusLed;
#else
        private readonly OutputPin HeaterRelay;
        private readonly OutputPin StatusLed;
#endif
        private readonly Configuration Config;
        private readonly ILogger<Thermostat> log;

        public Thermostat(Configuration _config, ILogger<Thermostat> _log)
        {
            Config = _config;
            log = _log;

            HeaterRelay = new(HeaterRelayPinNumber, log, "Heater Relay");
            StatusLed = new(StatusLedPinNumber, log, "Heater Led");
        }


        public async Task<APIMessage> Refresh(CancellationToken token)
        {
            var status = ReadValues(token).ConfigureAwait(false);
            var setpoint = GetCurrentSetpoint();

            (double? temperature, double? relativeHumidity) = await status;
            
            if (temperature.HasValue)
            {           
                var diff = Convert.ToDecimal(temperature.Value) - setpoint;

                if (Math.Abs(diff) > Config.Threshold)
                {
                    if (diff < 0)
                        StartHeating();
                    else
                        StopHeating();
                }
            }
            else
                //sensor failure => STACCAH STACCAH STACCAAAAAH
                StopHeating();

            var heaterTime = GetHeatingTime();
            var heaterIsActive = HeaterRelay.IsOn();

            int overrideSecondsRemaining = 0;
            if (Config.OverrideUntil.HasValue)
                overrideSecondsRemaining = Convert.ToInt32((Config.OverrideUntil.Value - new DateTime(1970, 1, 1)).TotalSeconds);

            Config.SaveConfiguration();

            var msg = new APIMessage()
            {
                CurrentSetpoint = setpoint,
                HeaterOn = heaterTime,
                Humidity = relativeHumidity,
                Temperature = temperature,
                Timestamp = DateTime.Now,
                IsHeaterOn = heaterIsActive,
                OverrideEnd = overrideSecondsRemaining
            };

            return msg;
        }

        public void OverrideSetpoint(decimal setpoint, long? untilEpoch)
        {
            Config.OverrideSetpoint = setpoint;

            if (untilEpoch.HasValue)
                Config.OverrideUntil = untilEpoch.Value.ToDateTime();
            else
                Config.OverrideUntil = DateTime.Now.AddHours(Config.OverrideDefaultDuration);
        }

        public void ReturnToProgram()
        {
            Config.OverrideSetpoint = null;
            Config.OverrideUntil = DateTime.Now.AddSeconds(-10);
        }

        public void SetAway()
        {
            Config.OverrideSetpoint = Config.AwaySetpoint;
            Config.OverrideUntil = DateTime.Now.AddYears(1);
        }

        #region Privates


        private bool IsSetpointOverridden()
        {
            if (Config.OverrideUntil.HasValue)
            {
                if (Config.OverrideUntil.Value >= DateTime.Now)
                    return true;
                else
                    Config.OverrideUntil = null;
            }

            return false;
        }

        private decimal GetCurrentSetpoint()
        {
            if (IsSetpointOverridden())
                return Config.OverrideSetpoint.Value;
            else
            {
                // program [monday] [08] [25 / 15 = 1]
                int dow = (int)DateTime.Now.DayOfWeek;
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                minute -= minute % 15;
                return Config.Program[dow][hour][minute];
            }
        }

        #endregion


        #region Hardware

        private async Task<(double? temperature, double? relativeHumidity)> ReadValues(CancellationToken token)
        {
#if DEBUG
            return await GenerateValues(token);
#endif
            bool tempOk = false;
            bool humOk = false;
            Temperature temp;
            RelativeHumidity hum;
            int wait = 2000;
            int loop = 1;

            using (var controller = new GpioController())
            {
                try
                {
                    var sensor = new Dht22(DhtPinNumber, PinNumberingScheme.Board, controller);

                    tempOk = sensor.TryReadTemperature(out temp);
                    humOk = sensor.TryReadHumidity(out hum);

                    humOk = humOk && hum.Percent >= 0 && hum.Percent <= 100;
                    tempOk = tempOk && humOk;

                    while (!tempOk || !humOk)
                    {
                        log.LogWarning("Sensor read failed");
                        if (wait < 4999)
                            wait += 500;
                        await Task.Delay(wait, token).ConfigureAwait(false);

                        if (!tempOk)
                            tempOk = sensor.TryReadTemperature(out temp);

                        if (!humOk)
                            humOk = sensor.TryReadHumidity(out hum);

                        humOk = humOk && hum.Percent >= 0 && hum.Percent <= 100;
                        tempOk = tempOk && humOk;

                        loop++;

                        if (loop > 20)
                            break;
                    }

                }
                catch (OperationCanceledException ex)
                {
                    log.LogError(ex, "Sensor read cancelled!");
                    return (null, null);
                }
            }

            if (loop > 20)
            {
                log.LogError("Sensor read cancelled!");
                return (null, null);
            }
            else
            {
                log.LogInformation("Sensor read OK");
                return (temp.DegreesCelsius, hum.Percent);
            }
        }

        private async ValueTask<(double? temperature, double? relativeHumidity)> GenerateValues(CancellationToken token)
        {
            log.LogInformation("Generated values");
            return (20, Random.Shared.Next(30, 90));
        }

        private void StartHeating()
        {

            if (!Config.HeaterOnSince.HasValue)
                Config.HeaterOnSince = DateTime.Now;

            HeaterRelay.TurnOn();
            StatusLed.TurnOn();
        }

        private void StopHeating()
        {
            HeaterRelay.TurnOff();
            StatusLed.TurnOff();
        }

        private int GetHeatingTime()
        {
            int seconds = 0;
            if (Config.HeaterOnSince.HasValue)
            {
                var ts = DateTime.Now;
                var delta = ts - Config.HeaterOnSince.Value;
                Config.HeaterOnSince = HeaterRelay.IsOn() ? ts : null;
                seconds = Convert.ToInt32(delta.TotalSeconds);
            }
            return seconds;
        }

        #endregion

    }
}
