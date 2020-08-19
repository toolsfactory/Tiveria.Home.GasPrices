using Tiveria.Home.GasPrices.TankerKoenig;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;
using Tiveria.Home.GasPrices.Interfaces;
using System;
using System.Linq;

namespace Tiveria.Home.GasPrices.CLI
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var config = CreateConfiguration(args);
            var services = ConfigureServices(args, config);
            var provider = services.BuildServiceProvider();

            var manager = provider.GetService<TankerKoenigManager>();

            Console.Clear();
            Console.WriteLine("Stations with Distance");
            Console.WriteLine();
            if (await manager.InitializeAsync(48.077750, 11.700687, 5))
            {
                foreach (var station in manager.OrderBy(x => x.Distance))
                {
                    Console.WriteLine($"Id: {station.ID} Name: {station.Brand} - {station.Name}, Diesel: {station.GasPrices[GasType.Diesel]}");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Stations with Id");
            Console.WriteLine();
            if (await manager.InitializeAsync(new string[] { "feb28bec-a110-4805-9739-3d2042711489", "186617f9-8861-4e0b-9908-2185ddb4f613", "bf488106-7f55-4ded-ab7d-ec130d8f2b2c" }))
            {
                foreach (var station in manager.OrderBy(x => x.Distance))
                {
                    Console.WriteLine($"Id: {station.ID} Name: {station.Brand} - {station.Name}, Diesel: {station.GasPrices[GasType.Diesel]}");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Stations after update");
            Console.WriteLine();
            if (await manager.UpdatePricesAsync())
            {
                foreach (var station in manager.OrderBy(x => x.Distance))
                {
                    Console.WriteLine($"Id: {station.ID} Name: {station.Brand} - {station.Name}, Diesel: {station.GasPrices[GasType.Diesel]}");
                }
            }
        }

        private static IServiceCollection ConfigureServices(string[] args, IConfiguration config)
        {
            var services = new ServiceCollection()
                .AddSingleton(CreateConfiguration(args))
                .AddLogging(logging =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(config, "Logging")
                        .CreateLogger();
                    logging.AddSerilog(logger);
                })
                .AddOptions()
                .Configure<TankerKoenigManagerOptions>(config.GetSection("TankerKoenig"))
                .AddSingleton<TankerKoenigManager>()
                .AddSingleton<HttpClient>()
                .AddHttpClient();

            return services;
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", true)
                        .AddCommandLine(args);
            return builder.Build();
        }
    }
}
