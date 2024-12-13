using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.TenantManagement;

namespace Omicx.QA.Services.Custom;

[Authorize]
[Route("api/app/tenant")]
public class TenantAppService : ApplicationService, ITenantAppService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantAppService(
        ITenantRepository tenantRepository
        )
    {
        _tenantRepository = tenantRepository;
    }
    
    [HttpPost("map-tenant")]
    public async Task MapTenant(Guid tenantId, int customTenantId)
    {
        try
        {
            var tenant = await _tenantRepository.FindAsync(tenantId);
            if (tenant is not null)
            {
                tenant.SetProperty("CustomTenantId", customTenantId);
                await _tenantRepository.UpdateAsync(tenant);
                Logger.LogInformation("Custom Tenant Updated");
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while mapping custom tenant to database");
        }
    }
}