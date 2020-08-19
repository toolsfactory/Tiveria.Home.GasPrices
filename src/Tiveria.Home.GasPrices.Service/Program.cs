using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Globalization;
using Tiveria.Home.GasPrices.TankerKoenig;
using System;

namespace Tiveria.Home.GasPrices.Service
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            await BuildHost(args).RunConsoleAsync().ConfigureAwait(false);
        }

        static IHostBuilder BuildHost(string[] args)
        {
            return new HostBuilder()
                .ConfigureHostConfiguration((config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", true)
#pragma warning disable CA1308 // Zeichenfolgen in Großbuchstaben normalisieren
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName.ToLower(CultureInfo.InvariantCulture)}.json", optional: true, reloadOnChange: true)
#pragma warning restore CA1308 // Zeichenfolgen in Großbuchstaben normalisieren
                        .AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddOptions()
                        .Configure<TankerKoenigManagerOptions>(hostContext.Configuration.GetSection("TankerKoenig"))
                        .Configure<ServiceOptions>(hostContext.Configuration.GetSection("Service"))
                        .AddSingleton<IHostedService, GasPricesService>()
                        .AddHttpClient();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .CreateLogger();
                    logging.AddSerilog(logger);
                })
                .UseSystemd();
        }
    }
}
