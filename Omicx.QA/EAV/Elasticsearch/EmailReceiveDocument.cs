using Nest;
using Omicx.QA.Elasticsearch.Documents;
using Omicx.QA.Entities;
using Omicx.QA.JsonRequests;
using Omicx.QA.JsonRequests.Call;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "email-receives", IdProperty = nameof(Id))]
public class EmailReceiveDocument : ElasticNestedEntity
{
    public Guid Id { get; set; }
    public required string EmailId { get; set; }
    public Guid TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public Dictionary<string, string>? Links { get; set; }
}