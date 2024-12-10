using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class DynamicEntitySchema : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual required string EntityType { get; set; }
    
    public virtual required string Label { get; set; }
    
    public virtual required string HashedProperties { get; set; }
}