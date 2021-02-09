using Notification.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notification.Data.Dto
{
    public class CampaignDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FromUser { get; set; }
        public List<string> RecepientMails { get; set; }
        public MessageDto Message { get; set;}

        public Campaign ToCampaign()
        {
            return new Campaign()
            {
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                FromUser = FromUser,
                Message = new Message()
                { 
                    ImageUri = this.Message.ImageUri,
                    CircleImageUri = this.Message.CircleUri,
                    Text = this.Message.Message,
                    Title = this.Message.Title
                },
                Recepients = this.RecepientMails.Select(p=>new NotificationUser() { Name = p }).ToList()
            };
        }

        public static CampaignDto FromCampaign(Campaign campaign)
        {
            throw new NotImplementedException();
        }
    }
}
