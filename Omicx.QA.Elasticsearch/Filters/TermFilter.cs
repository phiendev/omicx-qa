using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class TermFilter : Filter<object>
{
    public TermFilter(string field, object value) : base(field, value)
    {
    }

    public TermFilter((string, object) pair) : this(pair.Item1, pair.Item2)
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new TermQuery
    {
        Field = FieldIncludePrefix(prefix),
        Value = SingleValue
    };
}