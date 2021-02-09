using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Notification.AgentSmith
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    // Schedule
        //    // each {signalR.Reconnect.Delay} get list of user from all Engine agent
        //    // for all getter users, if last ping time > signalR.Reconnect.Delay -> remove user connection from redis cache / send disconnect command to engine agent
        //    Console.WriteLine("Hello World!");
        //}


        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
