using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.EAV.DynamicAttribute;

public class EntityAttribute<T> : FullAuditedAggregateRoot<Guid>, IEntityAttribute, IDynamicAttribute
{
    public virtual Guid? TenantId { get; set; }

    public virtual int? CustomTenantId { get; set; }

    [JsonIgnore]
    public virtual required T Entity { get; set; }

    public virtual int? DynamicAttributeId { get; set; }
    
    [BsonIgnore]
    public virtual DynamicAttribute? DynamicAttribute { get; set; }

    public virtual string? Value { get; set; }

    public object GetValue()
    {
        if (DynamicAttribute == null)
            throw new ArgumentException("DynamicAttribute required!");

        if (DynamicAttribute.Type == DynamicAttributeType.TEXT) return GetString();

        if (DynamicAttribute.Type == DynamicAttributeType.NUMBER) return GetDecimal();

        if (DynamicAttribute.Type == DynamicAttributeType.DATETIME) return GetDateTime();

        if (DynamicAttribute.Type == DynamicAttributeType.SELECT) return GetString();

        if (DynamicAttribute.Type == DynamicAttributeType.BOOLEAN && bool.TryParse(Value, out var b)) return b;

        if (DynamicAttribute.Type == DynamicAttributeType.SELECT) return GetString();

        if (DynamicAttribute.Type == DynamicAttributeType.BOOLEAN) return GetBoolean();

        throw new NotSupportedException($"AttributeType {DynamicAttribute.Type} not supported yet!");
    }

    public (string, object) GetProperty()
    {
        if (DynamicAttribute.Type == DynamicAttributeType.SELECT)
        {
            if (GetValue() == null)
            {
                return (DynamicAttribute.SystemName, GetValue());
            }

            return (DynamicAttribute.SystemName, GetValue().ToString().Split(","));
        }

        return (DynamicAttribute.SystemName, GetValue());
    }

    public string GetString() => Value;

    public int? GetInt()
    {
        return int.TryParse(Value, out var i) ? i : null;
    }

    public long? GetLong()
    {
        return long.TryParse(Value, out var i) ? i : null;
    }

    public decimal? GetDecimal()
    {
        return decimal.TryParse(Value, out var i) ? i : null;
    }

    public DateTimeOffset? GetDateTime()
    {
        return DateTimeOffset.TryParse(Value, out var dt) ? dt.ToUniversalTime() : null;
    }

    public bool? GetBoolean()
    {
        return bool.TryParse(Value, out var bl) ? bl : null;
    }
}