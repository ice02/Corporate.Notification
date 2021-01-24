﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Corporate.Chat.API.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Corporate.Chat.API.Context
{
	/// <summary>
	/// 
	/// </summary>
	public class ChatContext : DbContext
	{
		private readonly IWebHostEnvironment _env;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="env"></param>
		public ChatContext(IWebHostEnvironment env)
		{
			_env = env;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="optionsBuilder"></param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// get the configuration from the app settings
			var config = new ConfigurationBuilder()
				.SetBasePath(_env.ContentRootPath)
				.AddJsonFile($"appsettings.json")
				.AddJsonFile($"appsettings.{_env.EnvironmentName}.json", true)
				.Build();

			// define the database to use
			//optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
			optionsBuilder.UseInMemoryDatabase("Chat");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new UserChatConfig());
			modelBuilder.ApplyConfiguration(new MessageConfig());

		}

		/// <summary>
		/// 
		/// </summary>
		public DbSet<UserChat> UsersChat { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DbSet<Message> Messages { get; set; }
	}
}