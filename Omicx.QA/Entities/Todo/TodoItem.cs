using Volo.Abp.Domain.Entities;

namespace Omicx.QA.Entities.Todo;

public class TodoItem : BasicAggregateRoot<Guid>
{
    public string Text { get; set; } = string.Empty;
}