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

            var thermo = await Thermostat.Initialize();
            var hub = await Hub.Initialize("", thermo);

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
