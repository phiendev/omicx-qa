using MongoDB.Bson.Serialization.Attributes;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class AttributeGroup : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? TenantId { get; set; }
    
    public virtual int? CustomTenantId { get; set; }
    
    public virtual string? Name { get; set; }
    
    public virtual Guid? DynamicEntitySchemaId { get; }

    private readonly IList<DynamicAttribute> _dynamicAttributes = new List<DynamicAttribute>();
    
    public virtual ICollection<Guid> DynamicAttributeIds => _dynamicAttributes.Select(x => x.Id).ToList();
    
    public void Add(DynamicAttribute attribute)
    {
        _dynamicAttributes.Add(attribute);
    }

    public void Remove(DynamicAttribute attribute)
    {
        _dynamicAttributes.Remove(attribute);
    }
}