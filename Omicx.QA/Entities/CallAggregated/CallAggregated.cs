using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.Entities.CallAggregated;

public class CallAggregated : FullAuditedAggregateRoot<Guid> 
{
    public Guid TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public AggregateAssignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public List<CallAggregatedAttribute> CallAggregatedAttributes { get; set; } = new List<CallAggregatedAttribute>();
}

public class AggregateAssignee {
    public long AgentId { get; set; }
    public string? AgentName { get; set; }
}