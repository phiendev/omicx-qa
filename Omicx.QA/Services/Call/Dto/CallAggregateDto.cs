using Omicx.QA.Entities;
using Omicx.QA.Entities.CallAggregate;

namespace Omicx.QA.Services.Call.Dto;

public class CallAggregateDto
{
    public virtual required string CallId { get; set; }
    public virtual Guid? TenantId { get; set; }
    public virtual int? CustomTenantId { get; set; }
    public virtual Assignee? Assignee { get; set; }
    public virtual string? RecordingUrl { get; set; }
    public virtual string? Content { get; set; }
    public virtual object? Links { get; set; }
}