using System;
using System.Threading.Tasks;

namespace Nerdostat.Device
{
    class Thermostat
    {
        private int HeaterRelayPin = 21;
        private int StatusLedPin = 17;
        private int DhtPin = 4;

        private Configuration config { get; set; }


        public static async Task<Thermostat> Initialize()
        {
            var data = await Configuration.LoadConfiguration();
            return new Thermostat(data);
        }

        public async Task Refresh()
        {
            
        }

        // PRIVATES

        private Thermostat(Configuration _config)
        {
            config = _config;
        }

        private bool IsSetpointOverridden()
        {
            if (config.OverrideUntil.HasValue)
            {
                DateTime epoch = new DateTime(1970, 1, 1).AddSeconds(config.OverrideUntil.Value);
                if (epoch >= DateTime.Now)
                    return true;
            }

            return false;
        }

        private decimal GetCurrentSetpoint()
        {
            if (IsSetpointOverridden())
                return config.OverrideSetPoint;
            // else
            // {
            //     config.Program.Get
            // }
            
            return 0;
        }
    }
}
