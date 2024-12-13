using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.JsonRequests;

namespace Omicx.QA.Services.Call.Request;

public class CallAggregateRequest
{
    public Guid? Id { get; set; }
    public required string CallId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public List<DynamicAttributeValue>? DynamicAttributeValues { get; set; }
}