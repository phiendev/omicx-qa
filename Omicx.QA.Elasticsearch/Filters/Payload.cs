using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections;
using Omicx.QA.Elasticsearch.Enums;

namespace Omicx.QA.Elasticsearch.Filters;

public class Payload : IPayload
{
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public FilterType Type { get; set; }

    [JsonProperty("payload")] //
    #pragma warning disable
    private object _payload;
    #pragma warning restore

    public IPayload GetPayloadData() => GetPayload(_payload, Type);

    private IPayload GetPayload(object payload, FilterType type)
    {
        if (payload != null)
        {
            if (payload is JArray jArray)
            {
                var arrayPayload = new ArrayPayload { Type = type };
                foreach (var obj in jArray)
                {
                    var parentPayload = obj.ToObject<Payload>();
                    if (parentPayload == null) continue;

                    if (parentPayload.Type == FilterType.Filter)
                    {
                        arrayPayload.Add(obj.ToObject<SinglePayload>());
                        continue;
                    }

                    var childPayload = GetPayload(parentPayload._payload, parentPayload.Type);
                    if (childPayload == null) continue;

                    arrayPayload.Add(childPayload);
                }

                return arrayPayload;
            }

            if (payload is JObject jObject && FilterType.Filter == Type) return jObject.ToObject<SinglePayload>();
        }

        return null;
    }

    protected DataType GetValueType(object value)
    {
        if (value == null) return DataType.Null;

        var valueType = value.GetType();

        if (typeof(string) == valueType) return DataType.String;

        if (typeof(bool) == valueType) return DataType.Boolean;

        if (typeof(int) == valueType
            || typeof(long) == valueType
            || typeof(double) == valueType) return DataType.Number;

        if (typeof(DateTime) == valueType) return DataType.DateTime;

        if (value is Array arrayValue)
        {
            return arrayValue.Length == 0 ? DataType.Null : GetValueType(arrayValue.GetValue(0));
        }

        if (valueType == typeof(JArray))
        {
            var array = (JArray)value;

            return !array.Any() ? DataType.Null : GetValueType(array.ElementAt(0));
        }

        if (valueType == typeof(JValue))
        {
            var jValue = ((JValue)value).Value;

            return jValue == null ? DataType.Null : GetValueType(jValue);
        }

        return DataType.Unsupported;
    }
}

public class SinglePayload : Payload
{
    public string Field { get; set; }
    public Operator Operator { get; set; }
    public object Value { get; set; }

    public DataType DataType => GetValueType(Value);
}

public class ArrayPayload : Payload, IEnumerable<IPayload>
{
    private int _key = -1;
    private readonly IDictionary<int, IPayload> _payloads = new Dictionary<int, IPayload>();

    private IPayload this[int index] => index < 0 ? null : _payloads[index];

    public void Add(IPayload payload)
    {
        if (payload == null) return;

        _payloads.TryAdd(++_key, payload);
    }

    public IEnumerator<IPayload> GetEnumerator() => _payloads.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ICollection<IPayload> GetValues() => _payloads.Values;
}