using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class TermsFilter : Filter<object>
{
    public TermsFilter(string field, params object[] values) : base(field, values: values)
    {
    }

    public TermsFilter((string, object) pair) : this(pair.Item1, pair.Item2 as object[] ?? new[] {pair.Item2})
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new TermsQuery
    {
        Field = FieldIncludePrefix(prefix),
        Terms = MultipleValues
    };
}