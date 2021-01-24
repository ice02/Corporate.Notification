using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notification.Data.Model;

namespace Notification.Data.Context
{
	public class NotificationContext : DbContext
	{
		private readonly IConfiguration _configuration;

		public NotificationContext(IConfiguration configuration)
		{
			_configuration = configuration;
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
			optionsBuilder.UseInMemoryDatabase("Notification");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new UserChatConfig());
			modelBuilder.ApplyConfiguration(new MessageConfig());

		}

		public DbSet<UserChat> UsersChat { get; set; }
		public DbSet<Message> Messages { get; set; }
	}
}