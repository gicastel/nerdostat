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
    internal class HostedWorker : IHostedService
    {
        private readonly ILogger log;
        private readonly Thermostat thermo;
        private readonly HubManager hub;
        private readonly ThermoConfiguration config;
        private readonly SqliteDatastore sqlStore;
        private readonly Predictor predictor;
        private readonly IHostApplicationLifetime appLifetime;

        private Task _applicationTask;

        public HostedWorker(Thermostat _thermo, 
            HubManager _hub, 
            ThermoConfiguration _config, 
            SqliteDatastore _datastoreSql,
            Predictor _predictor,
            ILogger<HostedWorker> _log, 
            IHostApplicationLifetime _appLifetime)
        {
            thermo = _thermo;
            hub = _hub;
            config = _config;
            sqlStore = _datastoreSql;
            predictor = _predictor;
            log = _log;
            appLifetime = _appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("Starting...");

            CancellationTokenSource _cancellationTokenSource = null;

            appLifetime.ApplicationStarted.Register(() =>
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _applicationTask = Task.Run(async () =>
                {
                    bool regenConfig = false;
                    //if (args.Length > 0 && args[0] == "regenConfig")
                    //    regenConfig = true;

                    config.LoadConfiguration(regenConfig);

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        using var maxOperationTimeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                        maxOperationTimeout.CancelAfter(TimeSpan.FromSeconds((config.Interval * 60) - 15));
                        var delay = Task.Delay(config.Interval * 60 * 1000, _cancellationTokenSource.Token);

                        try
                        {
                            var message = await thermo.Refresh(maxOperationTimeout.Token);
#if RELEASE
                            if (message.Temperature.HasValue)
                            {
                                sqlStore.AddMessage(message);
                            }
#endif                 
                            if (predictor.IsModelReady)
                            { 
                                var prediction =  predictor.Predict(message);
                                message.PredictedTemperature = prediction;
                            }

                            var sendData = hub.TrySendMessage(message, maxOperationTimeout.Token);
                            //LET IT GOOOOOOOOOO
                            var retrain = Task.Run(() => predictor.Train()/*, maxOperationTimeout.Token*/);
                            await delay;
                        }
                        catch (OperationCanceledException) {  } //pass
                        catch (Exception ex) 
                        {
                            //pokemon handler
                            // we don't want a connection problem preventing the thermostat to work
                            log.LogError(ex, "Exception in main loop");
                        }
                    }

                    log.LogWarning("Application escaped the main loop");
                    appLifetime.StopApplication();
                });
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                config.SaveConfiguration();
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
