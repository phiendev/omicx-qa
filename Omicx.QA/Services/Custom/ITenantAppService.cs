namespace Omicx.QA.Services.Custom;

public interface ITenantAppService
{
    Task MapTenant(Guid tenantId, int customTenantId);
}