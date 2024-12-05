using Omicx.QA.Services.Todo.Dto;

namespace Omicx.QA.Services.Todo;

public interface ITodoAppService
{
    Task<List<TodoItemDto>> GetListAsync();
    Task<TodoItemDto> CreateAsync(TodoItemDto todo);
    Task DeleteAsync(Guid id);
}