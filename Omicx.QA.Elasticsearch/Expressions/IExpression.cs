using Nest;

namespace Omicx.QA.Elasticsearch.Expressions;

public interface IExpression
{
    QueryContainer GetQuery(string prefix = null);
}