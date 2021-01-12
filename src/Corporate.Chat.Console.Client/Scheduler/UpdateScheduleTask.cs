using Microsoft.Extensions.DependencyInjection;
using Onova;
using Onova.Services;
using System;
using System.Threading.Tasks;

namespace Corporate.Chat.Console.Client.Scheduler
{
    public class UpdateScheduleTask : ScheduledProcessor
    {
        public UpdateScheduleTask(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        protected override string Schedule => "*/2 * * * *"; // each 2 minutes  //" */10 * * * *" // Each 10 hours;

        public override async Task ProcessInScope(IServiceProvider serviceProvider)
        {
            // check for udpate
            // Configure to look for packages in specified directory and treat them as zips
            using (var manager = new UpdateManager(
                new WebPackageResolver("http://localhost:5000/api/AutoUpdate"),
                new ZipPackageExtractor()))
            {
                // Check for new version and, if available, perform full update and restart
                await manager.CheckPerformUpdateAsync();
            }

            //return Task.CompletedTask;
        }
    }
}
