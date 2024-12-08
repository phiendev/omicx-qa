using Nest;

namespace Omicx.QA.Elasticsearch.Expressions;

public class AndExpression : Expression
{
    public AndExpression(params IExpression[] expressions) : base(expressions)
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new BoolQuery
    {
        Must = Expressions.Select(e => e.GetQuery(prefix))
    };
}