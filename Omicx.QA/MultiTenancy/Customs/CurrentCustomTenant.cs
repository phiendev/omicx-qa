using MongoDB.Driver;
using Omicx.QA.Entities.Customs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Omicx.QA.Mongo;

namespace Omicx.QA.MultiTenancy.Customs;

public class CurrentCustomTenant : ITransientDependency, ICurrentCustomTenant
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IMongoDatabaseProvider _mongoDatabaseProvider;
    private const string CollectionName = "AbpTenants";

    public CurrentCustomTenant(
        ICurrentTenant currentTenant,
        IMongoDatabaseProvider mongoDatabaseProvider
        )
    {
        _currentTenant = currentTenant;
        _mongoDatabaseProvider = mongoDatabaseProvider;
    }
    
    public Guid? Id => _currentTenant.Id;
    public string? Name => _currentTenant.Name;
    public bool IsAvailable => _currentTenant.IsAvailable;
    public IDisposable Change(Guid? id, string? name = null)
    {
        return _currentTenant.Change(id, name);
    }
    public async Task<int?> GetCustomTenantIdAsync()
    {
        if (_currentTenant.Id.HasValue)
        {
            var database = await _mongoDatabaseProvider.GetDatabaseAsync();
            var collection = database.GetCollection<CustomTenant>(CollectionName);
            var filter = Builders<CustomTenant>.Filter.Eq(t => t.Id, _currentTenant.Id);
            var tenant = await collection.Find(filter).FirstOrDefaultAsync();
            if(tenant is not null) return tenant.CustomTenantId;
        }
        return null;
    }
}