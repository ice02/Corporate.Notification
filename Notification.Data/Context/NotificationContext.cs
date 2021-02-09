using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Notification.Data.Model;

namespace Notification.Data.Context
{
	public class NotificationContext : DbContext
	{
		//private readonly IConfiguration _configuration;

		//public NotificationContext(IConfiguration configuration)
		//{
		//	_configuration = configuration;
		//}

		private IConfiguration Configuration => new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.Build();

		public NotificationContext(): base()
		{ 

		}

		public NotificationContext(DbContextOptions<NotificationContext> options) : base(options)
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// get the configuration from the app settings
			//var config = new ConfigurationBuilder()
			//	.SetBasePath(_env.ContentRootPath)
			//	.AddJsonFile($"appsettings.json")
			//	.AddJsonFile($"appsettings.{_env.EnvironmentName}.json", true)
			//	.Build();

			// define the database to use
			//optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
			//optionsBuilder.UseInMemoryDatabase("Notification");
			optionsBuilder.UseNpgsql(Configuration.GetConnectionString("PgSqlConnection"));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new NotificationUserConfig());
			modelBuilder.ApplyConfiguration(new MessageConfig());

		}

		public DbSet<NotificationUser> UsersChat { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<Campaign> Campaigns { get; set; }
	}

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificationContext>
    {
        public NotificationContext CreateDbContext(string[] args)
        {
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(@Directory.GetCurrentDirectory() + "appsettings.json")
				.Build();
			var builder = new DbContextOptionsBuilder<NotificationContext>();
			var connectionString = configuration.GetConnectionString("PgSqlConnection");
			builder.UseNpgsql(connectionString);
			return new NotificationContext(builder.Options);
		}
    }
}