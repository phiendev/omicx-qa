using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.Entities.Todo;

public class TodoItem : FullAuditedAggregateRoot<Guid>
{
    public int? CustomTenantId { get; set; }
    public string Text { get; set; } = string.Empty;
}