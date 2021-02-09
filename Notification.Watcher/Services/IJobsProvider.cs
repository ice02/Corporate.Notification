using Notification.AgentSmith.Domain;
using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Services
{
    public interface IJobsProvider
    {
        Task<ICollection<QuartzJob>> GetScheduledJobs();
        Task ExecuteJob(string jobGroup, string jobName);
        Task PauseJob(string jobGroup, string jobName);

        Task CreateTriggerForJob(ITrigger trigger, IJobDetail job);
    }
}