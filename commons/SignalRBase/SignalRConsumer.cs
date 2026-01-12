using commons.EventBase;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace commons.SignalRBase;

public abstract class SignalRConsumer<THub>(
    ILogger logger,
    INotifier<THub> notifier,
    IConnectionMapping<THub> connectionMapping
    ) : CampusConsumer(logger) where THub : Hub
{
    protected readonly IConnectionMapping<THub> _connectionMapping = connectionMapping;
    protected readonly INotifier<THub> _notifier = notifier;

    protected bool IsUserOnline(string userId)
    {
        return _connectionMapping.GetConnections(userId).Any();
    }
}

public abstract class SignalRConsumer<THub, TResponse>(
    ILogger logger,
    INotifier<THub> notifier,
    IConnectionMapping<THub> connectionMapping
    ) : CampusConsumer<TResponse>(logger) where THub : Hub
{
    protected readonly IConnectionMapping<THub> _connectionMapping = connectionMapping;
    protected readonly INotifier<THub> _notifier = notifier;

    protected bool IsUserOnline(string userId)
    {
        return _connectionMapping.GetConnections(userId).Any();
    }
}