using Microsoft.Extensions.Logging;
using Notification.Data.Context;
using Notification.Data.Model;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Jobs
{
    public class OpenCampaignJob : IJob
    {
        private readonly ILogger<OpenCampaignJob> _logger;

        public OpenCampaignJob(ILogger<OpenCampaignJob> logger)
        {
            this._logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(context.JobDetail.Key + " job executing, triggered by " + context.Trigger.Key);

            var data = context.Trigger.JobDataMap;
            var campaignID = data["CampaignID"].ToString();
            _logger.LogInformation($"Execute OpenCampaign with ID {campaignID}");

            // get the list of users in the campaign
            using (NotificationContext ctx = new NotificationContext())
            {
                var campaign = ctx.Campaigns.FirstOrDefault(p => p.ID == int.Parse(campaignID));

                if (campaign == null)
                {
                    // TODO : Error campaign no exist
                }

                ctx.Entry(campaign).Collection(c => c.Recepients).Load();
                ctx.Entry(campaign).Reference(c => c.Message).Load();

                var mess = new MessageToSend()
                {
                    CircleImageUri = campaign.Message.CircleImageUri,
                    ImageUri = campaign.Message.ImageUri,
                    Text = campaign.Message.Text,
                    Title = campaign.Message.Title,
                    WithTextFeedback = campaign.Message.WithTextFeedback,
                    FromUser = campaign.FromUser
                };

                using (HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
                {
                    client.BaseAddress = new Uri($"https://localhost:7001/api/");

                    foreach (var usr in campaign.Recepients)
                    {
                        mess.ToUser = usr.Name;
                        // Send message to user througth engine Message endpoint
                        // TODO : Change user in campaign state based on api result
                        HttpResponseMessage response = await client.PostAsJsonAsync("messages", mess);
                    }
                    //campaign.Recepients.ToList().ForEach(async u =>
                    //{
                    //    mess.ToUser = u.Name;
                    //    // Send message to user througth engine Message endpoint
                    //    // TODO : Change user in campaign state based on api result
                    //    HttpResponseMessage response = await client.PostAsJsonAsync("", mess);
                    //});


                    // TODO: Rollback database if API error

                    //response.EnsureSuccessStatusCode();
                    //string responseBody = await response.Content.ReadAsStringAsync();

                    //TODO: manage result
                }

                //TODO: change campaign state
            }
            // check for pre requisits
            // send message to all users throught engine api
            // TODO : send report to campaign creator and/or given DL

        }

        //public Task Execute(IJobExecutionContext context)
        //{
            

        

        //    return null;
        //}
    }
}
