using Microsoft.AspNetCore.SignalR;
using commons.SignalRBase;
using Microsoft.Extensions.Logging;

namespace notification.Implementation;

public class HubNotifier<THub>(
    ILogger<HubNotifier<THub>> logger,
    IHubContext<THub> hubContext,
    IConnectionMapping<THub> connectionMapping
) : INotifier<THub> where THub : Hub
{
    private readonly ILogger<HubNotifier<THub>> _logger = logger;
    private readonly IHubContext<THub> _hubContext = hubContext;
    private readonly IConnectionMapping<THub> _connectionMapping = connectionMapping;

    public async Task AddUserToGroupAsync(string userId, string groupId)
    {
        var connections = _connectionMapping.GetConnections(userId);
        foreach (var connection in connections)
        {
            await _hubContext.Groups.AddToGroupAsync(connection, groupId);
        }
    }

    public async Task RemoveUserFromGroupAsync(string userId, string groupId)
    {
        var connections = _connectionMapping.GetConnections(userId);
        foreach (var connection in connections)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connection, groupId);
        }
    }

    public async Task SendToGroupAsync(string groupId, string method, object payload)
    {
        await _hubContext.Clients.Group(groupId).SendAsync(method, payload);
    }

    public async Task SendToUserAsync(string userId, string method, object payload)
    {
        var connections = _connectionMapping.GetConnections(userId);
        if (connections.Any())
        {
            await _hubContext.Clients.Users([.. connections]).SendAsync(method, payload);
        }
    }
}
