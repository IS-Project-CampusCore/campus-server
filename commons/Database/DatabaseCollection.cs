using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace commons.Database;

public class DatabaseCollection<T>(IMongoCollection<T> collection) : IDatabaseCollection<T> where T : DatabaseModel
{
    public IMongoCollection<T> MongoCollection => collection;

    public async Task<bool> DeleteWithIdAsync(string id)
    {
        var result = await collection.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, id));
        if (result.IsAcknowledged)
            return result.DeletedCount > 0;
        return false;
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => collection.Find(predicate).AnyAsync();

    public Task<T> GetOneAsync(Expression<Func<T, bool>> predicate) => collection.Find(predicate).SingleOrDefaultAsync();
    public Task<T> GetOneByIdAsync(string id) => collection.Find(x => x.Id == id).SingleOrDefaultAsync();

    public async Task<string> InsertAsync(T entity)
    {
        await collection.InsertOneAsync(entity);
        return entity.Id;
    }

    public Task ReplaceAsync(T entity) => ReplaceAsync(x => x.Id == entity.Id, entity);

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T entity) => collection.ReplaceOneAsync(predicate, entity);

    public Task UpdateAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> selector, TProperty value)
    {
        var filter = Builders<T>.Filter.Where(predicate);
        var update = Builders<T>.Update.Set(selector, value);
        return collection.UpdateManyAsync(filter, update);
    }

    public Task UpdateByIdAsync<TProperty>(string id, Expression<Func<T, TProperty>> selector, TProperty value)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var update = Builders<T>.Update.Set(selector, value);
        return collection.UpdateOneAsync(filter, update);
    }
    public async Task UpsertAsync(Expression<Func<T, bool>> predicate, T entity)
    {
        var filter = Builders<T>.Filter.Where(predicate);
        await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions
        {
            IsUpsert = true
        });
    }
}
