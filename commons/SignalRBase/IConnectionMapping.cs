using Microsoft.AspNetCore.SignalR;

namespace commons.SignalRBase;

public interface IConnectionMapping<THub> where THub : Hub
{
    void Add(string userId, string connectionId);
    void Remove(string userId, string connectionId);
    IEnumerable<string> GetConnections(string userId);
}
