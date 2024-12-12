using MongoDB.Driver;

namespace Omicx.QA.Mongo;

public interface IMongoDatabaseProvider
{
    Task<IMongoCollection<T>> GetCollectionAsync<T>(string collectionName);
}