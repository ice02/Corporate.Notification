using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Models
{
    public class JobDto
    {
        public string JobName { get; set; }
        public string Jobtype { get; set; }

        public string IsRecurrent { get; set; }
        public int RecurrencyValue { get; set; }
        public RecurrencyTypeEnum RecurrencyType { get; set; }

        public MisfirePolicy MisfirePolicy { get; set; }

        // null if start now
        public DateTime? StartDate { get; set; }
    }

    public enum MisfirePolicy
    {
    }

    public enum RecurrencyTypeEnum
    {
        seconds, minutes, Hours, days, weeks
    }
}
