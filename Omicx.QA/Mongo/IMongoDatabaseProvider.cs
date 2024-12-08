using MongoDB.Driver;

namespace Omicx.QA.Mongo;

public interface IMongoDatabaseProvider
{
    Task<IMongoDatabase> GetDatabaseAsync();
}