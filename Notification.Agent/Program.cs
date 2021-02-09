using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Display.Configuration;
using Notification.Display.Library;

namespace Notification.Display
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    //TODO: Add logging
                    //logging.AddConsole();
                })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddLogging();
                services.AddHostedService<NotificationWorker>();
            })
            .UseWindowsService(option => { option.ServiceName = "MDWNotifier"; })
            ;
    }
}
