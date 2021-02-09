using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Jobs
{
    /// <summary>
    /// clean Redis cache by removing ghost users
    /// </summary>
    public class KillEmAllJob : IJob
    {
        private readonly ILogger<KillEmAllJob> _logger;

        public KillEmAllJob(ILogger<KillEmAllJob> logger)
        {
            this._logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            // ask to each engine services for user heartbeats
            // remove from redis cache if heartbeat are too old

            _logger.LogDebug("Hello from KillEmAll");

            return null;
        }
    }
}
