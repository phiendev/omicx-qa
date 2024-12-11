using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities;

namespace Omicx.QA.Services.Call.Request;

public class CallAggregateRequest
{
    public required string CallId { get; set; }
    public Guid? TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public object? Links { get; set; }
    public List<DynamicAttributeValue>? DynamicAttributeValues { get; set; }
}