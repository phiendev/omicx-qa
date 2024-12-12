using Nest;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using Omicx.QA.Elasticsearch.Extensions;

namespace Omicx.QA.Elasticsearch.Documents;

public abstract class ElasticNestedEntity :
    NestedAttributes,
    IElasticNestedEntity,
    IReadOnlyDictionary<string, object>,
    IEnumerable<KeyValuePair<string, object>>,
    IEnumerable,
    IReadOnlyCollection<KeyValuePair<string, object>>
{
    private readonly IDictionary<string, PropertyInfo> _declaredAttributes =
        (IDictionary<string, PropertyInfo>)new Dictionary<string, PropertyInfo>();

    protected ElasticNestedEntity()
    {
        foreach (PropertyInfo propertyInfo in
                 ((IEnumerable<PropertyInfo>)this.GetType().GetProperties()).Where<PropertyInfo>(
                     (Func<PropertyInfo, bool>)(p =>
                         p.SetMethod != (MethodInfo)null && p.SetMethod.IsPublic &&
                         p.GetCustomAttribute<IgnoreAttribute>() == null)))
            this._declaredAttributes.TryAdd<string, PropertyInfo>(propertyInfo.Name, propertyInfo);
    }

    public object GetValue(string propName)
    {
        if (string.IsNullOrEmpty(propName))
            throw new ArgumentNullException(nameof(propName));
        if (this._declaredAttributes.ContainsKey(propName))
            return this._declaredAttributes[propName].GetValue((object)this);
        object obj;
        if (this.ContainsKey(propName) && this.TryGetValue(propName, out obj))
            return obj;
        throw new ArgumentException("Property '" + propName + "' not exists in '" + this.GetType().Name + "'");
    }

    public new void Add(string key, object value)
    {
        if (this._declaredAttributes.ContainsKey(key.UpperFirst()))
        {
            PropertyInfo declaredAttribute = this._declaredAttributes[key.UpperFirst()];
            Type type1 = Nullable.GetUnderlyingType(declaredAttribute.PropertyType);
            if ((object)type1 == null)
                type1 = declaredAttribute.PropertyType;
            Type type2 = type1;
            object safeValue = ElasticNestedEntity.GetSafeValue(value, type2);
            declaredAttribute.SetValue((object)this, safeValue);
        }

        base.Add(key, value);
    }

    public void AfterPropertiesSet()
    {
        foreach ((string key, PropertyInfo propertyInfo) in (IEnumerable<KeyValuePair<string, PropertyInfo>>)this
                     ._declaredAttributes)
            this.Add(key, propertyInfo.GetValue((object)this));
    }

    private static object GetSafeValue(object value, Type type)
    {
        if (value == null)
            return (object)null;
        if (type == typeof(Guid))
            return Guid.TryParse(value.ToString(), out var guidValue) ? guidValue : null;
        if (type.IsEnum)
            return Enum.TryParse(type, value.ToString(), true, out var enumValue) ? enumValue : null;
        if (type.IsClass && type != typeof(string))
        {
            if (value is JObject jObject)
            {
                return jObject.ToObject(type);
            }

            if (value is Dictionary<string, object> dictionary)
            {
                var jsonString = System.Text.Json.JsonSerializer.Serialize(dictionary);
                return System.Text.Json.JsonSerializer.Deserialize(jsonString, type);
            }
        }
        if (type.IsArray || type.IsGenericType)
            return value is JArray jarray ? jarray.ToObject(type) : value;
        if (type == typeof(Dictionary<string, string>))
        {
            if (value is JObject jObject)
            {
                return jObject.ToObject<Dictionary<string, string>>();
            }

            if (value is Dictionary<string, object> objDict)
            {
                return objDict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty
                );
            }

            if (value is string jsonString)
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            }
        }
        if (type != typeof(DateTimeOffset))
            return Convert.ChangeType(value, type);
        object safeValue;
        switch (value)
        {
            case DateTime dateTime:
                safeValue = !(dateTime == new DateTime()) ? (object)new DateTimeOffset(dateTime) : (object)null;
                break;
            case DateTimeOffset dateTimeOffset:
                if (dateTimeOffset == new DateTimeOffset())
                {
                    safeValue = (object)null;
                    break;
                }

                goto default;
            default:
                safeValue = Convert.ChangeType(value, type);
                break;
        }

        return safeValue;
    }
}