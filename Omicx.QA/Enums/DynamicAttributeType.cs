using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Omicx.QA.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum DynamicAttributeType
{
    TEXT,
    NUMBER,
    DATETIME,
    BOOLEAN,
    SELECT,
}