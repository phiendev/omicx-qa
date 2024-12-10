using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class DynamicEntitySchema : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual required string EntityType { get; set; }
    
    public virtual string? Label { get; set; }

    private readonly IList<Guid> _dynamicAttributes = new List<Guid>();
    
    public virtual ICollection<Guid> DynamicAttributeIds => _dynamicAttributes;
    
    private readonly IList<Guid> _attributeGroups = new List<Guid>();
    
    public virtual ICollection<Guid> AttributeGroupIds => _attributeGroups;
    
    public void Add(DynamicAttribute attribute)
    {
        _dynamicAttributes.Add(attribute.Id);
    }

    public void Add(AttributeGroup group)
    {
        _attributeGroups.Add(group.Id);
    }

    public void Remove(DynamicAttribute attribute)
    {
        _dynamicAttributes.Remove(attribute.Id);
    }

    public void Remove(AttributeGroup group)
    {
        _attributeGroups.Remove(group.Id);
    }
}