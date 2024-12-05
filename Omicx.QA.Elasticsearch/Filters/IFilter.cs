using Omicx.QA.Elasticsearch.Expressions;

namespace Omicx.QA.Elasticsearch.Filters;

public interface IFilter : IExpression
{
    string Field { get; }

    object[] Value { get; }
}