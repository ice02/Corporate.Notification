using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Domain
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QuartzApiJobParameterAttribute : Attribute
    {
        public string Name { get; }

        public QuartzApiJobParameterAttribute(string name)
        {
            Name = name;
        }
    }
}