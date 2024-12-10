using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "dynamic-entity-schemas", IdProperty = nameof(Id))]
public class DynamicEntitySchemaDocument : ElasticNestedEntity
{
    public Guid Id { get; set; }
}