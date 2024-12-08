using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "todo-items", IdProperty = nameof(Id))]
public class TodoItemDocument : ElasticNestedEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int CustomTenantId { get; set; }
    public string Text { get; set; } = string.Empty;
}