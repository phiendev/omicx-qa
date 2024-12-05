using System.Linq.Expressions;
using Nest;

namespace Omicx.QA.Elasticsearch.Configurations;

public interface IElasticsearchManager
{
    IElasticClient CreateClient();

    IElasticsearchManager Map<TDoc>(string indexName, Expression<Func<TDoc, object>> property) where TDoc : class;

    IElasticsearchManager Map<TDoc>(string indexName, string property) where TDoc : class;
        
    IElasticsearchManager MapDocuments();
}