using Omicx.QA.Services.Todo.Dto;
using Volo.Abp.Application.Services;

namespace Omicx.QA.Services.Todo;

public interface ITodoAppService : IApplicationService
{
    Task<List<TodoItemDto>> GetListAsync();
    Task<TodoItemDto> CreateAsync(TodoItemDto todo);
    Task DeleteAsync(Guid id);
    Task<string> HelloWorld();
}