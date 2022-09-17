using System;
using System.Threading.Tasks;

namespace Nerdostat.Device
{
    class Program
    {
        private const int Interval = 5*60;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");

            bool regenConfig = false;
            if (args.Length > 0 && args[0] == "regenConfig")
                regenConfig = true;

            var config = await Configuration.LoadConfiguration(regenConfig);
            var thermo = new Thermostat(config);
            if (config.IotHubConnectionString is null)
            {
                Console.WriteLine("WARN: Iot Connection String not configured");
            }

            var hub = await Hub.Initialize(config.IotHubConnectionString, config.TestDevice, thermo);

            while (true)
            {
                var message = await thermo.Refresh();

                try
                {
                    await hub.SendIotMessage(message);
                }
                catch 
                {
                    //pokemon handler
                    // we don't want a connection problem preventing the thermostat to work
                }

                await Task.Delay(Interval * 1000);
            }
        }
    }
}
