using MongoDB.Driver;
using Omicx.QA.Engine;
using Omicx.QA.Entities.Customs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Omicx.QA.MultiTenancy.Customs;

public class CurrentCustomTenant : ICurrentCustomTenant
{
    private readonly ICurrentTenant _currentTenant;
    private readonly MongoDbContext _context;

    public CurrentCustomTenant(
        ICurrentTenant currentTenant,
        MongoDbContext context
        )
    {
        _currentTenant = currentTenant;
        _context = context;
    }
    
    public async Task<int?> GetCustomTenantIdAsync()
    {
        if (_currentTenant.Id.HasValue)
        {
            var collection = _context.SharedData.Get<IMongoDatabase>("Database").GetCollection<CustomTenant>("AbpTenants");
            var currentCustomTenant = await collection.Find(x => x.Id == _currentTenant.Id).FirstOrDefaultAsync();
            return currentCustomTenant?.CustomTenantId;
        }

        return null;
    }
}