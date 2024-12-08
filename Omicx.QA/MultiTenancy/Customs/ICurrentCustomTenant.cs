using Volo.Abp.MultiTenancy;

namespace Omicx.QA.MultiTenancy.Customs;

public interface ICurrentCustomTenant : ICurrentTenant
{
    Task<int?> GetCustomTenantIdAsync();
}