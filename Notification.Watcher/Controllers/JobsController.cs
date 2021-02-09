using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.AgentSmith.Jobs;
using Notification.AgentSmith.Models;
using Notification.AgentSmith.Services;
using Notification.Data.Model;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Controllers
{
    [Route("api/{Scheduler}/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobsController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("{Group}/All")]
        public async Task<IActionResult> GetAll(string Scheduler, string Group)
        {
            var schedulersList = await _schedulerFactory.GetAllSchedulers();
            var scheduler = schedulersList.FirstOrDefault(p => p.SchedulerName == Scheduler);

            if (scheduler == null)
            {
                return NotFound($"Scheduler {Scheduler} was not found");
            }

            var result = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(Group));

            return Ok(result);
        }

        [HttpGet("{Group}/Detail/{Name}")]
        public async Task<IActionResult> GetDetail(string Scheduler, string Group, string Name)
        {
            var schedulersList = await _schedulerFactory.GetAllSchedulers();
            var scheduler = schedulersList.FirstOrDefault(p => p.SchedulerName == Scheduler);

            if (scheduler == null)
            {
                return NotFound($"Scheduler {Scheduler} was not found");
            }

            var result = await scheduler.GetJobDetail(new JobKey(Name, Group));

            return Ok(result);
        }

        // TODO: Add info in JobDto
        [HttpPost("{Group}/SimpleTrigger")]
        public async Task<IActionResult> SimplePost(string Group, string Scheduler, [FromBody] Campaign TriggerInfos)
        {
            var schedulersList = await _schedulerFactory.GetAllSchedulers();
            var scheduler = schedulersList.FirstOrDefault(p => p.SchedulerName == Scheduler);

            if (scheduler == null)
            {
                return NotFound($"Scheduler {Scheduler} was not found");
            }

            // TODO : Parameters validator
            // type of job
            // call abstract "validate" method for each job type

            //// get campaign from DB
            //// create open trigger and close trigger
            //// define the job and tie it to our HelloJob class
            // chack if job exist in the good group

            IJobDetail job = await scheduler.GetJobDetail(new JobKey($"OpenCampaign-{TriggerInfos.ID}", Group));
            if (job == null)
            {
                job = JobBuilder.Create<OpenCampaignJob>()
                   .WithIdentity($"OpenCampaign-{TriggerInfos.ID}", Group)
                   .Build();
            }

            //// Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"OpenCampaignTrigger-{TriggerInfos.ID}", Group)
                .ForJob(job)
                .UsingJobData("CampaignID", TriggerInfos.ID)
                .StartAt(TriggerInfos.StartDate)
                .Build();

            //// Tell quartz to schedule the job using our trigger
            var result = await scheduler.ScheduleJob(job, trigger);

            return Ok(result);

            // create trigger for campaign update check (if users were added or removed)
            // TODO: Create a Put to do that to be called by Put in campaign API
            // TODO : In the put call the diff job directly if date is over, do nothing if not
        }

        [HttpPost("{Group}/CalendarTrigger")]
        public async Task<IActionResult> CalendarPost(string Group, string Scheduler, [FromBody] JobDto TriggerInfos)
        {
            var schedulersList = await _schedulerFactory.GetAllSchedulers();
            var scheduler = schedulersList.FirstOrDefault(p => p.SchedulerName == Scheduler);

            if (scheduler == null)
            {
                return NotFound($"Scheduler {Scheduler} was not found");
            }

            // TODO : Parameters validator
            // type of job
            // call abstract "validate" method for each job type

            //// get campaign from DB
            //// create open trigger and close trigger
            //// define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<OpenCampaignJob>()
                .WithIdentity("OpenCampaign", Group)
                .Build();

            //// Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                //.WithCalendarIntervalSchedule(a => { a. })
                .WithIdentity("OpenCampaignTrigger", Group)
                .ForJob(job)
                .UsingJobData("CampaignID", "")
                .StartNow()
                .Build();

            //// Tell quartz to schedule the job using our trigger
            var result = await scheduler.ScheduleJob(job, trigger);

            return Ok(result);

            // create trigger for campaign update check (if users were added or removed)
            // TODO: Create a Put to do that to be called by Put in campaign API
            // TODO : In the put call the diff job directly if date is over, do nothing if not
        }

        [HttpPost("{Group}/CronTrigger")]
        public async Task<IActionResult> CronPost(string Group, string Scheduler, [FromBody] JobDto TriggerInfos)
        {
            var schedulersList = await _schedulerFactory.GetAllSchedulers();
            var scheduler = schedulersList.FirstOrDefault(p => p.SchedulerName == Scheduler);

            if (scheduler == null)
            {
                return NotFound($"Scheduler {Scheduler} was not found");
            }

            // TODO : Parameters validator
            // type of job
            // call abstract "validate" method for each job type

            //// get campaign from DB
            //// create open trigger and close trigger
            //// define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<OpenCampaignJob>()
                .WithIdentity("OpenCampaign", Group)
                .Build();

            //// Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("OpenCampaignTrigger", Group)
                .ForJob(job)
                .UsingJobData("CampaignID", "")
                .StartNow()
                .Build();

            //// Tell quartz to schedule the job using our trigger
            var result = await scheduler.ScheduleJob(job, trigger);

            return Ok(result);

            // create trigger for campaign update check (if users were added or removed)
            // TODO: Create a Put to do that to be called by Put in campaign API
            // TODO : In the put call the diff job directly if date is over, do nothing if not
        }
    }
}
