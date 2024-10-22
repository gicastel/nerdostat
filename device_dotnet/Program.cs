using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nerdostat.Device.Services;
using System.IO;
using System.Threading.Tasks;

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
                    services.AddHostedService<HostedWorker>()
                        .AddSingleton<ThermoConfiguration>()
                        .AddSingleton<Thermostat>()
                        .AddSingleton<HubManager>()
                        .AddSingleton<SqliteDatastore>()
                        .AddSingleton<Predictor>()
                        .AddSingleton<Microsoft.Data.Sqlite.SqliteFactory>();

                })
                .RunConsoleAsync();
        }
    }
}
