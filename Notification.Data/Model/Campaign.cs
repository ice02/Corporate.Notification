using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Data.Model
{
    public class Campaign
    {
        public int ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FromUser { get; set; }
        public bool IsActive { get; set; }
        public ICollection<NotificationUser> Recepients { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
}
