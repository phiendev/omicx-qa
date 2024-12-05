using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class MatchFilter : Filter<string>
{
    public MatchFilter(string field, params string[] values) : base(field, values: values)
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new MatchQuery
    {
        Field = FieldIncludePrefix(prefix),
        Query = SingleValue
    };
}