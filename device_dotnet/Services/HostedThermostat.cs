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
        private readonly IHostApplicationLifetime _appLifetime;

        private Task? _applicationTask;
        private const int Interval = 5 * 60;

        public HostedThermostat(Thermostat _thermo, Hub _hub, Configuration _config, ILogger<HostedThermostat> _log, IHostApplicationLifetime appLifetime)
        {
            Thermo = _thermo;
            Hub = _hub;
            Config = _config;
            log = _log;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("Starting...");

            CancellationTokenSource? _cancellationTokenSource = null;

            _appLifetime.ApplicationStarted.Register(() =>
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _applicationTask = Task.Run(async () =>
                {
                    bool regenConfig = false;
                    //if (args.Length > 0 && args[0] == "regenConfig")
                    //    regenConfig = true;

                    await Config.LoadConfiguration(regenConfig);

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        var message = await Thermo.Refresh();

                        try
                        {
                            Hub.SendIotMessage(message);
                            await Task.Delay(Interval * 1000, _cancellationTokenSource.Token);
                        }
                        catch
                        {
                            //pokemon handler
                            // we don't want a connection problem preventing the thermostat to work
                        }
                    }

                    _appLifetime.StopApplication();
                });
            });

            _appLifetime.ApplicationStopping.Register(() =>
            {
                log.LogInformation("Application is stopping");
                _cancellationTokenSource?.Cancel();
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_applicationTask != null)
            {
                await _applicationTask;
            }

            log.LogInformation($"Exiting...");
        }
    }
}
