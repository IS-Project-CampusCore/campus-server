using commons.Protos;


namespace commons;

public class ServiceMessageException(string msg, ServiceMessageException.MessageResponseDelegate generator) : Exception(msg)
{
    public delegate MessageResponse MessageResponseDelegate(string msg, object? obj = null);
    private readonly MessageResponseDelegate _generator = generator;

    public virtual MessageResponse ToResponse() => _generator(Message);
};

public class BadRequestException(string msg) : ServiceMessageException(msg, MessageResponse.BadRequest);
public class UnauthorizedException(string msg) : ServiceMessageException(msg, MessageResponse.Unauthorized);
public class ForbiddenException(string msg) : ServiceMessageException(msg, MessageResponse.Forbidden);
public class NotFoundException(string msg) : ServiceMessageException(msg, MessageResponse.NotFound);
public class InternalErrorException(string msg) : ServiceMessageException(msg, MessageResponse.Error);
