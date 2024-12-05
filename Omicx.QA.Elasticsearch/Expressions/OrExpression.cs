using Nest;

namespace Omicx.QA.Elasticsearch.Expressions;

public class OrExpression : Expression
{
    public OrExpression(params IExpression[] expressions) : base(expressions)
    {
        
    }
    
    public override QueryContainer GetQuery(string prefix = null)
    {
        throw new NotImplementedException();
    }
}