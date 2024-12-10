using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class EntityAttribute<T> : FullAuditedAggregateRoot<Guid>, IEntityAttribute, IDynamicAttribute
{
    public virtual Guid? TenantId { get; set; }

    public virtual int? CustomTenantId { get; set; }

    public virtual required string EntityType { get; set; }
    
    public virtual int DynamicAttributeId { get; set; }
    
    public virtual required string SystemName { get; set; }
    
    public virtual required DynamicAttributeType Type { get; set; }

    public virtual required string Value { get; set; }
    
    public (string, object) GetProperty()
    {
        return (SystemName, Type);
    }
}