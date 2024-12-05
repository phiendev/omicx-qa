using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class MultiMatchFilter : Filter<string>
{
    private readonly string[] _fields;

    public MultiMatchFilter(
        string value, params string[] fields) : base(string.Join(";", fields), value)
    {
        if (fields.Length == 0) throw new ArgumentException("Required at least field");

        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

        _fields = fields;
    }

    public override QueryContainer GetQuery(string prefix = null) => new MultiMatchQuery
    {
        Fields = _fields.Select(field => FieldIncludePrefix(prefix, field)).ToArray(),
        Query = SingleValue
    };
}