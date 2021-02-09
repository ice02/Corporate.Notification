﻿using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notification.Data.Context;
using Notification.Data.Model;
using Notification.Engine.Hubs;
using Notification.Engine.Providers;
using StackExchange.Redis;

namespace Notification.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            services.AddOptions();

            services.AddControllers();

            services.AddMvc(config =>
                {

                    config.Filters.Add(new ProducesAttribute("application/json"));
                    //config.Filters.Add(new ConsumesAttribute("application/json"));

                })
                .AddFluentValidation()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.WriteIndented = false;
                    x.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                    x.JsonSerializerOptions.IgnoreNullValues = false;
                });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

            services.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());

            services.AddSignalR()
                .AddJsonProtocol(x =>
                {
                    x.PayloadSerializerOptions.IgnoreNullValues = false;
                    x.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
                    x.PayloadSerializerOptions.WriteIndented = false;
                })
                .AddMessagePackProtocol()
                .AddStackExchangeRedis(o =>
                {
                    o.ConnectionFactory = async writer =>
                    {
                    var config = new StackExchange.Redis.ConfigurationOptions
                    {
                        AbortOnConnectFail = false,
                        ResolveDns = true
                    };

                        config.EndPoints.Add(Configuration.GetConnectionString("Redis"), 6379);
                        config.SetDefaultPorts();
                        var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                        connection.ConnectionFailed += (_, e) =>
                        {
                            Console.WriteLine("Connection to Redis failed.");
                        };

                        if (!connection.IsConnected)
                        {
                            Console.WriteLine("Did not connect to Redis.");
                        }

                        return connection;
                    };
                });

            //services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

            services.AddCors();


            services.AddEntityFrameworkInMemoryDatabase()
            //services.AddEntityFrameworkSqlServer()
                .AddDbContext<NotificationContext>((serviceProvider, options) =>
                {
                    options.UseApplicationServiceProvider(serviceProvider);
                    options.UseInternalServiceProvider(serviceProvider);
                    options.UseInMemoryDatabase("Notification");//.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<NotificationContext>();
            
            services.AddSingleton<IClientList, ClientList>();
            services.AddSingleton<ValidateAuthentication>();

            // Health Checks
            services.AddHealthChecks()
            //.AddSqlServer(Configuration.GetConnectionString("DefaultConnection"), name: "MSSQL")
                .AddRedis(Configuration.GetConnectionString("Redis"), name: "REDIS");

            services
                .AddHealthChecksUI(setup =>
                {
                    setup.SetHeaderText("Notification engine health checks");
                    //Maximum history entries by endpoint
                    setup.MaximumHistoryEntriesPerEndpoint(50);
                    //One endpoint is configured in appsettings, let's add another one programatically
                    setup.AddHealthCheckEndpoint("Endpoint2", "/healthchecks-data-ui");
                })
                //.AddSqliteStorage("Data Source = healthchecks.db");
                .AddInMemoryStorage();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notifications Engine Api", Version = "v1" });
                c.EnableAnnotations();
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(policy =>
            {
                var origins = Configuration.GetSection("Cors:Host")
                    .AsEnumerable()
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .Select(x => x.Value).ToArray();

                policy.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseResponseCompression();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<ValidateAuthentication>();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications API V1");
            });

            app.UseHttpsRedirection();

            // Dashboard health check UI
            app.UseHealthChecksUI();

            
            //Middlweare Health Check
            app.UseHealthChecks("/status",
                new HealthCheckOptions()
                {
                    ResponseWriter = async(context, report) =>
                    {
                        var result = JsonConvert.SerializeObject(
                            new
                            {
                                statusApplication = report.Status.ToString(),
                                    healthChecks = report.Entries.Select(e => new
                                    {
                                        check = e.Key,
                                            ErrorMessage = e.Value.Exception?.Message,
                                            status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                                    })
                            });
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });

            // Endpoint for dashboard
            app.UseHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
            {
                Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            

            // Ensure SQL Database Created
            using(var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using(var context = serviceScope.ServiceProvider.GetService<NotificationContext>())
            {
                try
                {
                    context.Database.EnsureCreated();
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapHub<NotificationHub>("/notification");
                endpoints.MapControllers();

                endpoints.MapHealthChecks("healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
                endpoints.MapDefaultControllerRoute();;

            });

            app.Run(context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

        }
    }
}