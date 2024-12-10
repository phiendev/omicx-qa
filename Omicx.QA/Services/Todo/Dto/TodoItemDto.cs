namespace Omicx.QA.Services.Todo.Dto;

public class TodoItemDto
{
    public Guid Id { get; set; }
    public int? CustomTenantId { get; set; }
    public string Text { get; set; } = string.Empty;
}