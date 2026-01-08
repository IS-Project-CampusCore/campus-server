using commons.EventBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.SignalRBase;

public interface ISignalRDefinition : IConsumerDefinition
{
    static abstract string Message { get; }
    static abstract object Content { get; }
}
