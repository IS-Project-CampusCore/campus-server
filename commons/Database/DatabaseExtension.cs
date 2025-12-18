using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace commons.Database;

public static class DatabaseExtension
{
    public static IServiceCollection AddMongoDatabase(this IServiceCollection services, string connectionString, string databaseName)
    {
        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(connectionString);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddSingleton<IDatabase, Database>();

        return services;
    }
}
