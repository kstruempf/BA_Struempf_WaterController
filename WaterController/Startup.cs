using System;
using System.Threading.Tasks;
using ContextBrokerLibrary.Api;
using Hangfire;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using WaterController.Services;
using WaterController.Services.Impl;

namespace WaterController
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setting up hangfire
            var hangfireConnectionString =
                Configuration.GetConnectionString("HangfireDb");

            if (string.IsNullOrEmpty(hangfireConnectionString))
            {
                throw new Exception("No hangfire connection string configured (HangfireDb)");
            }

            services.AddHangfire(configuration =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings();

                var retryCount = 0;


                while (true)
                {
                    try
                    {
                        configuration.UseStorage(new MySqlStorage(
                            hangfireConnectionString,
                            new MySqlStorageOptions
                            {
                                TablePrefix = "Hangfire"
                            }
                        ));
                        break;
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine($"Hangfire database connection failed ({e.Message}) - retry {retryCount}");

                        retryCount++;

                        if (retryCount == 10)
                        {
                            throw;
                        }
                    }

                    Task.Delay(2500).Wait();
                }
            });

            // Setting default headers for all requests to context broker
            ContextBrokerLibrary.Client.Configuration.Default.AddDefaultHeader("fiware-service",
                Configuration.GetValue<string>("FiwareService"));
            ContextBrokerLibrary.Client.Configuration.Default.AddDefaultHeader("fiware-servicepath",
                Configuration.GetValue<string>("FiwareServicePath"));
            ContextBrokerLibrary.Client.Configuration.Default.BasePath = GetBasePath();

            services.AddControllers();

            services.AddTransient<IEntityService, EntityService>();
            services.AddTransient<IValveService, ValveService>();

            services.AddSingleton<IEntitiesApi>(new EntitiesApi(ContextBrokerLibrary.Client.Configuration.Default));

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            app.UseHangfireServer();

            app.UseHangfireDashboard();

            SetupRecurringJobs();
        }

        private string GetBasePath()
        {
            return Configuration.GetValue<string>("ContextBrokerPath", null);
        }

        private void SetupRecurringJobs()
        {
            RecurringJob.AddOrUpdate<IValveService>("CheckValveJob", s => s.OpenValveIfRequired(),
                Configuration.GetValue("CheckValveCronExpr", "0,30 * * * *"));
        }
    }
}