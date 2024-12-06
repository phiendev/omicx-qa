namespace Omicx.QA.MultiTenancy.Customs;

public interface ICurrentCustomTenant
{
    Task<int?> GetCustomTenantIdAsync();
}