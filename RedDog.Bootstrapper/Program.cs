using System;
using System.Collections.Generic;
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

        static async Task Main(string[] args)
        {
            Program p = new Program();

            using AccountingContext context = p.CreateDbContext(null);
            await context.Database.MigrateAsync();
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
                }
                catch(Exception e)
                {
                    Console.WriteLine($"An exception occured while retrieving the secret from the Dapr sidecar. Retrying in 5 seconds...");
                    Console.WriteLine(e.StackTrace);
                    Task.Delay(5000).Wait();
                }
            } while(connectionString == null);

            DbContextOptionsBuilder<AccountingContext> optionsBuilder = new DbContextOptionsBuilder<AccountingContext>().UseSqlServer(connectionString["reddog-sql"], b => 
            {
                b.MigrationsAssembly("RedDog.Bootstrapper");
                b.EnableRetryOnFailure();
            });

            return new AccountingContext(optionsBuilder.Options);
        }
    }
}
