using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace commons.Database;

public class Database(IMongoDatabase mongoDatabase) : IDatabase
{
    private readonly ConcurrentDictionary<Type, object> _cache = [];

    public IDatabaseCollection<T> GetCollection<T>() where T : DatabaseModel
    {
        if (_cache.TryGetValue(typeof(T), out var chaced))
        {
            return (IDatabaseCollection<T>)chaced;
        }

        var attr = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
        if (attr is null)
        {
            throw new DatabaseException($"Model {typeof(T).Name} is missing [CollectionName] attribute");
        }

        var nativeCollection = mongoDatabase.GetCollection<T>(attr.Name);

        var wrapper = new DatabaseCollection<T>(nativeCollection);
        _cache[typeof(T)] = wrapper;

        return wrapper;
    }
}
