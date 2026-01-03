using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.Events;

public record MessageCreatedEvent(string SenderId, string GroupId, string? Content, List<string>? FilesId, DateTime Timestamp);
