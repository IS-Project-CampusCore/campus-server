using commons.SignalRBase;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace notification.Implementation;

public class ConnectionMapping<THub> : IConnectionMapping<THub> where THub : Hub
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = [];

    public void Add(string userId, string connectionId)
    {
        _connections.AddOrUpdate(userId,
            key => new HashSet<string>() { connectionId },
            (key, existingSet) =>
            {
                lock (existingSet) { existingSet.Add(connectionId); }
                return existingSet;
            });
    }

    public void Remove(string userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                    _connections.TryRemove(userId, out _);
            }
        }
    }

    public IEnumerable<string> GetConnections(string userId)
    {
        if (_connections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }

        return Enumerable.Empty<string>();
    }
}
