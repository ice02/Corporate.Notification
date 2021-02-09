using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Display.Library;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace Notification.Display.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private IHost _host;

        public App()
        {
            _host = new HostBuilder()
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(context.HostingEnvironment.ContentRootPath);
                    configurationBuilder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    //services.AddSingleton<ITextService, TextService>();
                    services.AddSingleton<MainWindow>();
                    services.AddHostedService<NotificationWorker>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .Build();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            await _host.StartAsync();
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
