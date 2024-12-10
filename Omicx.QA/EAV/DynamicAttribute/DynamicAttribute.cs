using Omicx.QA.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class DynamicAttribute : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual Guid? DynamicEntitySchemaId { get; set; }

    public virtual Guid? AttributeGroupId { get; set; }
    
    public virtual required DynamicAttributeType Type { get; set; }
    
    public virtual required string SystemName { get; set; }

    public virtual required string DisplayName { get; set; }

    public virtual string? DesignerOptions { get; set; }
    
    public virtual bool IsActive { get; set; }
    
    public virtual int? Order { get; set; }
}