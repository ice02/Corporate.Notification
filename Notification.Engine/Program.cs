using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Notification.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
		public static void Main(string[] args)
		{
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("./NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("Application Starting Up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
                if (args.Length > 0)
                {
                    webBuilder.UseUrls($"http://localhost:{args[0]}/");
                }
            }).ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog();
    }
}