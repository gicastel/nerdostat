using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nerdostat.Device.Services
{
    internal class HostedThermostat : IHostedService
    {
        private readonly ILogger log;
        private readonly Thermostat Thermo;
        private readonly Hub Hub;
        private readonly Configuration Config;

        private const int Interval = 5 * 60;

        public HostedThermostat(Thermostat _thermo, Hub _hub, Configuration _config, ILogger<HostedThermostat> _log)
        {
            Thermo = _thermo;
            Hub = _hub;
            Config = _config;
            log = _log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("Starting...");

            bool regenConfig = false;
            //if (args.Length > 0 && args[0] == "regenConfig")
            //    regenConfig = true;

            await Config.LoadConfiguration(regenConfig);
            await Hub.Initialize();

            //var config = await Configuration.LoadConfiguration(regenConfig);
            //var thermo = new Thermostat(config);
            //if (config.IotHubConnectionString is null)
            //{
            //    Console.WriteLine("WARN: Iot Connection String not configured");
            //}

            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await Thermo.Refresh();

                try
                {
                    await Hub.SendIotMessage(message);
                }
                catch
                {
                    //pokemon handler
                    // we don't want a connection problem preventing the thermostat to work
                }

                await Task.Delay(Interval * 1000);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
