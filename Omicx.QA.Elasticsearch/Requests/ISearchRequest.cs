using Omicx.QA.Elasticsearch.Expressions;

namespace Omicx.QA.Elasticsearch.Requests;

public interface ISearchRequest : IElasticRequest
{
    IExpression Expression { get; }
}

public interface ISearchRequest<T> : ISearchRequest
{
}