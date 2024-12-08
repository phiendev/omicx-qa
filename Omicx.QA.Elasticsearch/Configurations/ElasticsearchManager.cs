using System.Linq.Expressions;
using System.Reflection;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Omicx.QA.Elasticsearch.Extensions;

namespace Omicx.QA.Elasticsearch.Configurations;

public class ElasticsearchManager : IElasticsearchManager
{
    private readonly IDictionary<string, Type> _types = new Dictionary<string, Type>();
    private readonly ConnectionSettings _connectionSettings;

    public ElasticsearchManager(
        IElasticsearchConfiguration elasticsearchConfiguration)
    {
        _connectionSettings = InitializeConnectionSettings(elasticsearchConfiguration);
    }

    public IElasticClient CreateClient()
    {
        return new ElasticClient(_connectionSettings);
    }

    public IElasticsearchManager Map<TDoc>(
        string indexName,
        Expression<Func<TDoc, object>> property) where TDoc : class
    {
        _connectionSettings.DefaultMappingFor<TDoc>(
            dt => dt
                .IndexName(indexName)
                .RelationName(indexName)
                .IdProperty(property)
        );

        AddType(indexName, typeof(TDoc));

        return this;
    }

    public IElasticsearchManager Map<TDoc>(string indexName, string property) where TDoc : class
    {
        _connectionSettings.DefaultMappingFor<TDoc>(
            dt => dt
                .IndexName(indexName)
                .RelationName(indexName)
                .IdProperty(property)
        );

        AddType(indexName, typeof(TDoc));

        return this;
    }

    public IElasticsearchManager MapDocuments()
    {
        var types = AppDomain.CurrentDomain.GetTypesOfAttribute<ElasticsearchTypeAttribute>();
        foreach (var type in types)
        {
            var esIndex = type.GetCustomAttribute<ElasticsearchTypeAttribute>();
            if (esIndex == null || string.IsNullOrEmpty(esIndex.RelationName)) continue;
            _connectionSettings
                .DefaultMappingFor(type, descriptor =>
                {
                    descriptor.RelationName(esIndex.RelationName);
                    descriptor.IndexName(esIndex.RelationName);
                    if (!string.IsNullOrEmpty(esIndex.IdProperty))
                    {
                        descriptor.IdProperty(esIndex.IdProperty);
                    }

                    return descriptor;
                });

            AddType(esIndex.RelationName, type);
        }

        return this;
    }

    public IEnumerable<string> IndexNames => _types.Keys;

    private static IConnectionPool ElasticConnectionPool(string connectionStrings = "http://localhost:9200")
    {
        var nodes = connectionStrings
            .Split(',')
            .Select(s => new Uri(s))
            .ToList();

        if (nodes.Count == 1)
        {
            return new SingleNodeConnectionPool(nodes.First());
        }

        return nodes.Count > 1 ? new SniffingConnectionPool(nodes) : null;
    }


    private ConnectionSettings InitializeConnectionSettings(IElasticsearchConfiguration config)
    {
        var nodePool = ElasticConnectionPool(config.ConnectionStrings);
        var connectionSettings = new ConnectionSettings(nodePool, JsonNetSerializer.Default);
        connectionSettings.EnableApiVersioningHeader();

        if (!string.IsNullOrEmpty(config.Username))
            connectionSettings.BasicAuthentication(config.Username, config.Password);

        if (!string.IsNullOrEmpty(config.DefaultIndex))
        {
            connectionSettings.DefaultIndex(config.DefaultIndex);
        }

        return connectionSettings;
    }

    private void AddType(string indexName, Type docType)
    {
        if (_types.ContainsKey(indexName))
        {
            _types.Remove(indexName);
        }

        _types.Add(indexName, docType);
    }
}