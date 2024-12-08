using System.Reflection;
using Nest;
using Newtonsoft.Json.Linq;
using Omicx.QA.Elasticsearch.Extensions;

namespace Omicx.QA.Elasticsearch.Documents;

public abstract class ElasticNestedEntity : NestedAttributes, IElasticNestedEntity
{
    protected ElasticNestedEntity()
    {
        var properties = GetType()
            .GetProperties()
            .Where(p => p.SetMethod != null
                        && p.SetMethod.IsPublic
                        && p.GetCustomAttribute<IgnoreAttribute>() == null);
        foreach (var prop in properties) _declaredAttributes.TryAdd(prop.Name, prop);
    }

    private readonly IDictionary<string, PropertyInfo> _declaredAttributes = new Dictionary<string, PropertyInfo>();

    public object GetValue(string propName)
    {
        if (string.IsNullOrEmpty(propName)) throw new ArgumentNullException(nameof(propName));

        if (_declaredAttributes.ContainsKey(propName)) return _declaredAttributes[propName].GetValue(this);

        if (ContainsKey(propName) && TryGetValue(propName, out var value)) return value;

        throw new ArgumentException($"Property '{propName}' not exists in '{GetType().Name}'");
    }

    public new void Add(string key, object value)
    {
        if (_declaredAttributes.ContainsKey(key.UpperFirst()))
        {
            var propInfo = _declaredAttributes[key.UpperFirst()];
            var type = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
            var safeValue = GetSafeValue(value, type);

            propInfo.SetValue(this, safeValue);
        }

        base.Add(key, value);
    }

    public void AfterPropertiesSet()
    {
        foreach (var (field, info) in _declaredAttributes) Add(field, info.GetValue(this));
    }

    private static object GetSafeValue(object value, Type type)
    {
        if (value == null) return null;

        if (type.IsArray || type.IsGenericType)
        {
            if (value is JArray array) return array.ToObject(type);
            return value;
        }

        if (type != typeof(DateTimeOffset)) return Convert.ChangeType(value, type);

        return value switch
        {
            DateTime dateTime when dateTime == default => null,
            DateTime dateTime => new DateTimeOffset(dateTime),
            DateTimeOffset dateTimeOffset when dateTimeOffset == default => null,
            _ => Convert.ChangeType(value, type)
        };
    }
}