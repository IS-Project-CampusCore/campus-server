using Microsoft.AspNetCore.SignalR;
using commons.SignalRBase;
using commons;
using Microsoft.Extensions.Logging;

namespace notification.Hubs;

public class CampusHub<THub>(
    ILogger logger,
    IConnectionMapping<THub> connectionMapping
) : Hub where THub : Hub
{
    protected readonly ILogger _logger = logger;
    protected readonly IConnectionMapping<THub> _connectionMapping = connectionMapping;

    protected UserJwt? CurrentUser => GetUserFromClaims();

    public override async Task OnConnectedAsync()
    {
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["Handler"] = GetType().Name,
            ["UserId"] = CurrentUser?.Id,
            ["ConnectionId"] = Context.ConnectionId
        }))
        {
            if (CurrentUser is not null)
            {
                _connectionMapping.Add(CurrentUser.Id, Context.ConnectionId);
                _logger.LogInformation($"User:{CurrentUser.Name} connected to Hub:{GetType().Name}");
            }
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["Handler"] = GetType().Name,
            ["UserId"] = CurrentUser?.Id,
            ["ConnectionId"] = Context.ConnectionId
        }))
        {
            if (CurrentUser is not null)
            {
                _connectionMapping.Remove(CurrentUser.Id, Context.ConnectionId);
                _logger.LogInformation($"User:{CurrentUser.Name} disconnected from Hub:{GetType().Name}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }

    private UserJwt? GetUserFromClaims()
    {
        if (Context.User?.Claims is not null)
        {
            try
            {
                return UserJwtExtensions.FromClaims(Context.User.Claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user from Claims failed with Exception:{ExName}, Msg:{ExMsg}", ex.GetType().Name, ex.Message);
            }
        }

        return null;
    }
}
