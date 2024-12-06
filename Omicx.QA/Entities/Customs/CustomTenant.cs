using Volo.Abp.TenantManagement;

namespace Omicx.QA.Entities.Customs;

public class CustomTenant : Tenant
{
    public int CustomTenantId { get; set; }
}