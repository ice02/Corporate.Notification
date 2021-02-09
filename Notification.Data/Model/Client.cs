using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Data.Model
{
    public class Client
    {
        public Client(string connectionId, string userId)
        {
            ConnectionId = connectionId;
            EntranceTime = DateTime.Now;
            UserId = userId;
        }

        public string UserId { get; set; }

        public string ConnectionId { get; }

        public DateTime EntranceTime { get; }
        public DateTime LatestPingTime { get; set; }
        public DateTime ExitTime { get; internal set; }

    }
}
