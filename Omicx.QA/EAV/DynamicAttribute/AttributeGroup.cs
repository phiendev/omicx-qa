using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class AttributeGroup : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual string? AttributeGroupName { get; set; }
    
    public virtual Guid? DynamicEntitySchemaId { get; set; }
}