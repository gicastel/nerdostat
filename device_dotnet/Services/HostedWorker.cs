using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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
        // private readonly Predictor predictor;
        // private readonly MeteoService meteo;
        private readonly IHostApplicationLifetime appLifetime;

        private Task _applicationTask;

        public HostedWorker(Thermostat _thermo, 
            HubManager _hub, 
            ThermoConfiguration _config, 
            SqliteDatastore _datastoreSql,
            // Predictor _predictor,
            // MeteoService _meteo,

            ILogger<HostedWorker> _log, 
            IHostApplicationLifetime _appLifetime)
        {
            thermo = _thermo;
            hub = _hub;
            config = _config;
            sqlStore = _datastoreSql;
            // predictor = _predictor;
            // meteo = _meteo;
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

                    // disable meteo service for now                  
                    // try
                    // {
                    //     await meteo.Initialize();
                    // }
                    // catch (Exception ex)
                    // {
                    //     log.LogError(ex, "Failed to initialize meteo service");
                    // }

                    // disable predictor for now
                    // var retrain = Task.Run(async () =>
                    // {
                    //     while (!_cancellationTokenSource.IsCancellationRequested)
                    //     {
                    //         try
                    //         {
                    //             var training = Task.Delay(TimeSpan.FromHours(24), _cancellationTokenSource.Token);
                    //             predictor.Train(_cancellationTokenSource.Token);
                    //             await training;
                    //         }
                    //         catch (OperationCanceledException) { } //pass
                    //         catch (Exception ex)
                    //         {
                    //             log.LogError(ex, "Exception in retrain loop");
                    //         }
                    //     }
                    // });

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        using var maxOperationTimeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);

                        maxOperationTimeout.CancelAfter(TimeSpan.FromSeconds((config.Interval * 60) - 15));
                        var delay = Task.Delay(config.Interval * 60 * 1000, _cancellationTokenSource.Token);

                        using var predictTimeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                        predictTimeout.CancelAfter(TimeSpan.FromSeconds(config.Interval * 60 - 30));

                        try
                        {
                            var message = await thermo.Refresh(maxOperationTimeout.Token);

                            // var prediction = await predictor.Predict(predictTimeout.Token);
                            // message.PredictedTemperature = prediction;

#if RELEASE
                            if (message.Temperature.HasValue)
                            {
                                sqlStore.AddMessage(message);
                            }
#endif

                            var sendData = hub.TrySendMessage(message, maxOperationTimeout.Token);
                            //LET IT GOOOOOOOOOO

                            if (message.SensorFailures > 19)
                            {
                                log.LogError("Too many sensor failures, restarting...");
                                await Task.WhenAll([sendData]);
                                Process.Start(new ProcessStartInfo() { FileName = "sudo", Arguments = "reboot" });
                            }
                            await delay;
                        }
                        catch (OperationCanceledException) { } //pass
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
