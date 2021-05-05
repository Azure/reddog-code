using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RedDog.AccountingService
{
    public class Program
    {
        private const string SecretStoreName = "reddog.secretstore";

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With(new UtcTimestampEnricher())
                //.Destructure.ByTransforming<CustomerOrder>(co => new { StoreId = co.StoreId, FirstName = co.FirstName, LastName = co.LastName, LoyaltyId = co.LoyaltyId, OrderItemCount = co.OrderItems.Count })
                //.Destructure.ByTransforming<OrderSummary>(os => new { OrderId = os.OrderId, StoreId = os.StoreId, FirstName = os.FirstName, LastName = os.LastName, LoyaltyId = os.LoyaltyId, OrderItemCount = os.OrderItems.Count, OrderTotal = os.OrderTotal })
                .WriteTo.Console(outputTemplate: "[{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                CreateHostBuilder(args).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(config =>
                {
                    var daprClient = new DaprClientBuilder().Build();
                    var secretDescriptors = new List<DaprSecretDescriptor>
                    {
                        new DaprSecretDescriptor("reddog-sql")
                    };
                    config.AddDaprSecretStore(SecretStoreName, secretDescriptors, daprClient);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    class UtcTimestampEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime));
        }
    }
}
