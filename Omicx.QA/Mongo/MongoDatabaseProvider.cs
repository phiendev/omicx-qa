using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Omicx.QA.Mongo;

public class MongoDatabaseProvider : ITransientDependency, IMongoDatabaseProvider
{
    private readonly IConnectionStringResolver _connectionStringResolver;

    public MongoDatabaseProvider(
        IConnectionStringResolver connectionStringResolver)
    {
        _connectionStringResolver = connectionStringResolver;
    }

    public async Task<IMongoDatabase> GetDatabaseAsync()
    {
        var connectionString = await _connectionStringResolver.ResolveAsync("Default");
        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        return client.GetDatabase(mongoUrl.DatabaseName);
    }
}