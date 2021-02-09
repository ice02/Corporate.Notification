using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Notification.AgentSmith.Jobs;
using Notification.AgentSmith.Services;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith
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
            //services.AddOpenTelemetryTracing(builder =>
            //{
            //    builder
            //        .AddQuartzInstrumentation()
            //        .AddZipkinExporter(o =>
            //        {
            //            o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            //            o.ServiceName = "Quartz.Examples.AspNetCore";
            //        })
            //        .AddJaegerExporter(o =>
            //        {
            //            o.ServiceName = "Quartz.Examples.AspNetCore";

            //            // these are the defaults
            //            o.AgentHost = "localhost";
            //            o.AgentPort = 6831;
            //        });
            //});

            // base configuration for DI
            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            services.AddQuartz(q =>
            {
                q.SchedulerId = "Scheduler-Notifier";

                q.UseMicrosoftDependencyInjectionJobFactory(options =>
                {
                    // if we don't have the job in DI, allow fallback to configure via default constructor
                    options.AllowDefaultConstructor = true;
                    options.CreateScope = false;
                });

                // these are the defaults
                q.UseSimpleTypeLoader();
                q.UsePersistentStore(p =>
                {
                    p.UseJsonSerializer();
                    p.UseClustering(c => { });
                    p.UsePostgres("Server=localhost;Port=5432;Database=scheduler;Username=postgres;Password=mysecretpassword");
                });
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });


                var jobKey = new JobKey("CacheCleanerJob", "Notification");
                q.AddJob<KillEmAllJob>(j => j
                    .WithIdentity(jobKey)
                    .WithDescription("Redis cache cleaner job")
                );

                q.AddTrigger(t => t
                        .WithIdentity("CacheCleaner Trigger")
                        .ForJob(jobKey)
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(10)).RepeatForever())
                        .WithDescription("Redis cache cleaner")
                    );

                // base quartz scheduler, job and trigger configuration
                // TODO : Add kill'em all job each 6 minutes to keed cache clean
            });

            services.AddTransient<OpenCampaignJob>();
            services.AddTransient<CloseCampaignJob>();
            //services.AddTransient<KillEmAllJob>();

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });


            


            //services.AddSingleton<IScheduler>(SetupScheduler().Result);
            //services.AddTransient<IJobsProvider, JobsProvider>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Agent Smith Api", Version = "v1" });
            });

            //services
            //    .AddHealthChecksUI()
            //    .AddInMemoryStorage();
        }

        public async Task<IScheduler> SetupScheduler()
        {
            //var stdSchedulerFactory = new StdSchedulerFactory();
            //var scheduler = stdSchedulerFactory.GetScheduler().Result;

            //scheduler.Start();

            var scheduler = await SchedulerRepository.Instance.Lookup("QuartzScheduler");

            return scheduler;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agent Smith v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
