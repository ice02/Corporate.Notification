using System;
using System.Collections.Generic;

namespace Notification.Data.Model
{
    public class NotificationUser
    {
        public int ID { get; set; }

        public string ConnectionId { get; set; }
        public string Name { get; set; }
        
		public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}