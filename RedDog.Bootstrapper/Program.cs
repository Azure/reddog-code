using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RedDog.AccountingModel;

namespace RedDog.Bootstrapper
{
    class Program : IDesignTimeDbContextFactory<AccountingContext>
    {
        private const string SecretStoreName = "reddog.secretstore";
        private string DaprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
        private HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Beginning EF Core migrations...");
            Program p = new Program();

            using AccountingContext context = p.CreateDbContext(null);
            await context.Database.MigrateAsync();

            Console.WriteLine("Migrations complete.");
        }

        public AccountingContext CreateDbContext(string[] args)
        {
            string connectionString = GetDbConnectionString().Result;

            DbContextOptionsBuilder<AccountingContext> optionsBuilder = new DbContextOptionsBuilder<AccountingContext>().UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("RedDog.Bootstrapper");
                b.EnableRetryOnFailure();
            });

            return new AccountingContext(optionsBuilder.Options);
        }

        private async Task EnsureDaprOrTerminate()
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{DaprHttpPort}/v1.0/healthz");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error communicating with Dapr sidecar. Exiting...", e.InnerException?.Message ?? e.Message);
                Environment.Exit(1);
            }
        }

        private void ShutdownDapr()
        {
            Console.WriteLine("Attempting to shutdown Dapr sidecar...");
            bool isDaprShutdownSuccessful = false;
            HttpClient httpClient = new HttpClient();
            do
            {
                try
                {
                    var response = httpClient.PostAsync($"http://localhost:{DaprHttpPort}/v1.0/shutdown", null).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        isDaprShutdownSuccessful = true;
                        Console.WriteLine("Successfully shutdown Dapr sidecar.");
                    }
                    else
                    {
                        Console.WriteLine($"Unable to shutdown Dapr sidecar. Retrying in 5 seconds...");
                        Console.WriteLine($"Dapr error message: {response.Content.ReadAsStringAsync().Result}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An exception occured while attempting to shutdown the Dapr sidecar.");
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    Task.Delay(5000).Wait();
                }
            } while (!isDaprShutdownSuccessful);
        }

        private async Task<string> GetDbConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("reddog-sql");

            if (connectionString == null)
            {
                EnsureDaprOrTerminate().Wait();

                Console.WriteLine("Attempting to retrieve connection string from Dapr secret store...");
                var daprClient = new DaprClientBuilder().Build();

                Dictionary<string, string> connectionStringSecret = null;
                do
                {
                    try
                    {
                        Console.WriteLine("Attempting to retrieve database connection string from Dapr...");
                        connectionStringSecret = await daprClient.GetSecretAsync(SecretStoreName, "reddog-sql");
                        Console.WriteLine("Successfully retrieved database connection string.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"An exception occured while retrieving the secret from the Dapr sidecar. Retrying in 5 seconds...");
                        Console.WriteLine(e.InnerException?.Message ?? e.Message);
                        Console.WriteLine(e.StackTrace);
                        Task.Delay(5000).Wait();
                    }
                } while (connectionStringSecret == null);
                connectionString = connectionStringSecret["reddog-sql"];
                ShutdownDapr();
            }

            return connectionString;
        }
    }
}
