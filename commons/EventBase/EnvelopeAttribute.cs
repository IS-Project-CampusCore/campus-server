using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.EventBase;

[AttributeUsage(AttributeTargets.Class)]
public class EnvelopeAttribute(string eventName) : Attribute
{
    public string EventName { get; } = eventName;
}
