using Nest;
using Omicx.QA.Elasticsearch.Expressions;

namespace Omicx.QA.Elasticsearch.Filters;

public abstract class Filter<TVal> : Expression, IFilter
{
    public string Field { get; }
    public object[] Value { get; }

    private TVal[] Values { get; }

    protected Filter(string field, TVal[] values)
    {
        if (values == null || values.Length == 0) throw new ArgumentNullException(nameof(values));

        Field = field;
        Value = values.Select(tv => (object) tv).ToArray();
        Values = values;

        AddExpression(this);
    }

    protected Filter(string field, TVal value) : this(field, value == null ? Array.Empty<TVal>() : new[] {value})
    {
            
    }

    public abstract override QueryContainer GetQuery(string prefix = null);

    protected TVal SingleValue => Values == null || Values.Length == 0 ? default : Values[0];
    protected TVal[] MultipleValues => Values?.Where(v => v != null).ToArray();

    protected string FieldIncludePrefix(string prefix = null, string field = null)
    {
        var usageField = string.IsNullOrEmpty(field) ? Field : field;

        return string.IsNullOrEmpty(prefix) ? usageField : $"{prefix}.{usageField}";
    }
}