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
            var daprClient = new DaprClientBuilder().Build();

            Dictionary<string, string> connectionString = null;
            do
            {
                try
                {
                    connectionString = daprClient.GetSecretAsync(SecretStoreName, "reddog-sql").GetAwaiter().GetResult();
                    Console.WriteLine($"connectionString={connectionString}");
                }
                catch(Exception e)
                {
                    Console.WriteLine($"An exception occured while retrieving the secret from the Dapr sidecar. Retrying in 5 seconds...");
                    Console.WriteLine($"connectionString={connectionString}");
                    Console.WriteLine(e.StackTrace);
                    Task.Delay(5000).Wait();
                }
            } while(connectionString == null);

            bool isDaprShutdownSuccessful = false;
            HttpClient httpClient = new HttpClient();
            do
            {
                try
                {
                    var response = httpClient.PostAsync($"http://localhost:{DaprHttpPort}/v1.0/shutdown", null).Result;

                    if(response.IsSuccessStatusCode)
                    {
                        isDaprShutdownSuccessful = true;
                    }
                    else
                    {
                        Console.WriteLine($"Unable to shutdown Dapr sidecare. Retrying in 5 seconds...");
                        Console.WriteLine($"Dapr error message: {response.Content.ReadAsStringAsync().Result}");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"An exception occured while attempting to shutdown the Dapr sidecar.");
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    Task.Delay(5000).Wait();
                }
            } while(!isDaprShutdownSuccessful);

            DbContextOptionsBuilder<AccountingContext> optionsBuilder = new DbContextOptionsBuilder<AccountingContext>().UseSqlServer(connectionString["reddog-sql"], b => 
            {
                b.MigrationsAssembly("RedDog.Bootstrapper");
                b.EnableRetryOnFailure();
            });

            return new AccountingContext(optionsBuilder.Options);
        }
    }
}
