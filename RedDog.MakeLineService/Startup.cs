using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace RedDog.MakeLineService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers().AddDapr();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RedDog.MakeLineService", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RedDog.MakeLineService v1"));
            app.UseSerilogRequestLogging();
            app.UseRouting();
            app.UseCloudEvents();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapControllers();
            });
        }
    }
}
