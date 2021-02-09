using Notification.AgentSmith.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Domain
{
    public class QuartzJob
    {
        public String Name { get; set; }
        public string Group { get; set; }
        public IList<QuartzTrigger> Triggers { get; set; }
        public string[] Parameters { get; set; }
    }
}
