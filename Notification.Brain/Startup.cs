using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Notification.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notification.Brain.Services;
using System.IO;

namespace Notification.Brain
{
    public class Startup
    {
        private IConfiguration Configuration => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        public Startup()
        {
            //Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notifications Brain Api", Version = "v1" });
            });

            services.AddEntityFrameworkNpgsql()
            //services.AddEntityFrameworkInMemoryDatabase()
            //services.AddEntityFrameworkSqlServer()
                .AddDbContext<NotificationContext>((serviceProvider, options) =>
                {
                    options.UseApplicationServiceProvider(serviceProvider);
                    options.UseInternalServiceProvider(serviceProvider);
                    //options.UseInMemoryDatabase("Notification");//.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                    options.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection"));

                });

            services.AddScoped<CampaignService>();

            //services.AddDbContext<NotificationContext>(options =>
            //        options.UseNpgsql(Configuration.GetConnectionString("postgre")))
            //    .AddUnitOfWork<DiogelContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Brain v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // Ensure SQL Database Created
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = serviceScope.ServiceProvider.GetService<NotificationContext>())
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
