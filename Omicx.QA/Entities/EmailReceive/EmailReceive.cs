using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.Entities.EmailReceive;

public class EmailReceive : FullAuditedAggregateRoot<Guid> 
{
    public virtual required string EmailId { get; set; }
    public virtual Guid? TenantId { get; set; }
    public virtual int? CustomTenantId { get; set; }
    public virtual Assignee? Assignee { get; set; }
    public virtual string? RecordingUrl { get; set; }
    public virtual string? Content { get; set; }
    public virtual object? Links { get; set; }
    public virtual List<EmailReceiveAttribute>? EmailReceiveAttributes { get; set; }
}