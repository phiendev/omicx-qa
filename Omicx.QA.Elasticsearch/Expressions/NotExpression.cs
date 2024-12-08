using Nest;

namespace Omicx.QA.Elasticsearch.Expressions;

public class NotExpression : Expression
{
    public NotExpression(params IExpression[] expressions) : base(expressions)
    {
    }

    public override QueryContainer GetQuery(string prefix = null) => new BoolQuery
    {
        MustNot = Expressions.Select(e => e.GetQuery(prefix))
    };
}