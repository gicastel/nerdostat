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
        private readonly HubManager Hub;
        private readonly Configuration Config;
        private readonly IHostApplicationLifetime _appLifetime;

        private Task _applicationTask;

        public HostedThermostat(Thermostat _thermo, HubManager _hub, Configuration _config, ILogger<HostedThermostat> _log, IHostApplicationLifetime appLifetime)
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

            CancellationTokenSource _cancellationTokenSource = null;

            _appLifetime.ApplicationStarted.Register(() =>
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _applicationTask = Task.Run(async () =>
                {
                    bool regenConfig = false;
                    //if (args.Length > 0 && args[0] == "regenConfig")
                    //    regenConfig = true;

                    Config.LoadConfiguration(regenConfig);

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        using var maxOperationTimeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                        maxOperationTimeout.CancelAfter(TimeSpan.FromSeconds((Config.Interval * 60) - 15));
                        var delay = Task.Delay(Config.Interval * 60 * 1000, _cancellationTokenSource.Token);

                        try
                        {
                            var message = await Thermo.Refresh(maxOperationTimeout.Token);
                            var sendData = Hub.TrySendMessage(message, maxOperationTimeout.Token);
                            await delay;
                        }
                        catch (OperationCanceledException) {  } //pass
                        catch (Exception ex) 
                        {
                            //pokemon handler
                            // we don't want a connection problem preventing the thermostat to work
                            log.LogError("Exception in main loop", ex);
                        }
                    }

                    log.LogWarning("Application escaped the main loop");
                    _appLifetime.StopApplication();
                });
            });

            _appLifetime.ApplicationStopping.Register(() =>
            {
                Config.SaveConfiguration();
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
