using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nerdostat.Device.Services;

namespace Nerdostat.Device
{
    class Program
    {
        private const int Interval = 5 * 60;
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<HostedThermostat>()
                        .AddSingleton<Configuration>()
                        .AddSingleton<Thermostat>()
                        .AddSingleton<Hub>();
                })
                .RunConsoleAsync();
        }
    }
}
