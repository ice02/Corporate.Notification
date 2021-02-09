using Microsoft.AspNetCore.SignalR.Client;
using Notification.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Brain.Services
{
    public class CampaignService
    {
        private readonly NotificationContext _context;
        private readonly HubConnection _hubConnection;


        public CampaignService(NotificationContext context)
        {
            _context = context;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/notification", option => { option.UseDefaultCredentials = true; })
                .WithAutomaticReconnect()
                .ConfigureLogging(factory =>
                {
                    // TODO :  add logs
                    //factory.AddConsole();
                    //factory.AddFilter("Console", level => level >= LogLevel.Trace);
                }).Build();

            _hubConnection.StartAsync();
        }

        public async Task CheckForOpenCampaign()
        {
            // close opened campaign with a dead enddate
            var campaignToClose = _context.Campaigns.Where(p => p.IsActive == true && p.EndDate < DateTime.UtcNow);
            campaignToClose.ToList().ForEach(q => q.IsActive = false);

            // check if we have to send messages
            var campaigns = _context.Campaigns.Where(p=>p.IsActive == true && p.StartDate < DateTime.UtcNow && p.EndDate > DateTime.UtcNow);
            foreach (var campaign in campaigns)
            {
                foreach (var user in campaign.Recepients)
                {
                    // send message to user
                    await _hubConnection.SendAsync("SendToUser", campaign.Message, user);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
