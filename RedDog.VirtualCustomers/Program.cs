using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RedDog.VirtualCustomers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Debug)
                //.MinimumLevel.Override("System.Net.Http.HttpClient.Default.LogicalHandler", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.With(new UtcTimestampEnricher())
                .WriteTo.Console(outputTemplate: "[{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                using (IHost host = new HostBuilder()
                    .ConfigureHostConfiguration(configHost =>
                    {
                        configHost.SetBasePath(Directory.GetCurrentDirectory());
                        configHost.AddEnvironmentVariables(prefix: "DOTNETCORE_");
                        configHost.AddCommandLine(args);
                    })
                    .ConfigureAppConfiguration((hostContext, configApp) =>
                    {
                        configApp.SetBasePath(Directory.GetCurrentDirectory());
                        configApp.AddEnvironmentVariables(prefix: "DOTNETCORE_");
                        configApp.AddJsonFile($"appsettings.json", true);
                        configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                        configApp.AddCommandLine(args);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddDaprClient();
                        services.AddHostedService<VirtualCustomers>();
                    })
                    .UseSerilog()
                    .Build())
                {
                    await host.RunAsync();
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }

    class UtcTimestampEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime));
        }
    }
}