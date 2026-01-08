using Microsoft.AspNetCore.SignalR;

namespace commons.SignalRBase;

public interface INotifier<THub> where THub : Hub
{
    Task SendToUserAsync(string userId, string method, object payload);
    Task SendToGroupAsync(string groupId, string method, object payload);
    Task AddUserToGroupAsync(string userId, string groupId);
    Task RemoveUserFromGroupAsync(string userId, string groupId);
}
