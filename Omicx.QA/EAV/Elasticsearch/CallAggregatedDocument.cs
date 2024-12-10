using Nest;
using Omicx.QA.Elasticsearch.Documents;
using Omicx.QA.Entities.CallAggregated;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "todo-items", IdProperty = nameof(Id))]
public class CallAggregatedDocument : ElasticNestedEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public AggregateAssignee? AggregateAssignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
}