using Nest;
using Omicx.QA.Elasticsearch.Expressions;

namespace Omicx.QA.Elasticsearch.Filters;

public class NestedFilter : Filter<IExpression>
{
    private readonly IExpression _value;

    public NestedFilter(string field, params IExpression[] values) : base(field, values)
    {
        _value = SingleValue ?? throw new ArgumentNullException(nameof(values));
    }

    public override QueryContainer GetQuery(string prefix = null)
    {
        return new NestedQuery
        {
            Path = Field,
            Query = _value.GetQuery(Field)
        };
    }
}