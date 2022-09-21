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
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .UseContentRoot(Path.GetDirectoryName(System.AppContext.BaseDirectory))
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
