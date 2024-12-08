using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "todo-items", IdProperty = nameof(Id))]
public class TodoItemDocument : ElasticNestedEntity
{
    public long Id { get; set; }
    public string TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public string Text { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public DateTime? CreationTime { get; set; }
    public string? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string? LastModifierId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public string? DeleterId { get; set; }
}