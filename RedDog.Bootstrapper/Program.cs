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
            var connectionString = daprClient.GetSecretAsync(SecretStoreName, "reddog-sql").GetAwaiter().GetResult();

            DbContextOptionsBuilder<AccountingContext> optionsBuilder = new DbContextOptionsBuilder<AccountingContext>().UseSqlServer(connectionString["reddog-sql"], b => 
            {
                b.MigrationsAssembly("RedDog.Bootstrapper");
                b.EnableRetryOnFailure();
            });

            return new AccountingContext(optionsBuilder.Options);
        }
    }
}
