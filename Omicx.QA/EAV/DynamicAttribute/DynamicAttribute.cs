using MongoDB.Bson.Serialization.Attributes;
using Omicx.QA.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class DynamicAttribute : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual Guid? DynamicEntitySchemaId { get; }

    public virtual Guid? AttributeGroupId { get; }
    
    public virtual required string SystemName { get; set; }
    
    public virtual DynamicAttributeType Type { get; set; }

    public virtual required string DisplayName { get; set; }

    public virtual string? DesignerOptions { get; set; }
    
    public virtual bool IsActive { get; set; }
}