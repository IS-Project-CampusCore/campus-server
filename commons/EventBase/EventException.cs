using commons.Protos;
using commons.RequestBase;

namespace commons.EventBase;

public class EventException : ServiceMessageException
{
    public EventException(string message, MessageResponseDelegate generator)
        : base($"[Event System] {message}", generator) { }
}

public class EventValidationException(string msg) : EventException(msg, MessageResponse.BadRequest);
public class EventForbiddenException(string msg) : EventException(msg, MessageResponse.Forbidden);
public class EventUnauthorizedException(string msg) : EventException(msg, MessageResponse.Unauthorized);
public class EventNotFoundException(string msg) : EventException(msg, MessageResponse.NotFound);
public class EventErrorException(string msg) : EventException(msg, MessageResponse.Error);
