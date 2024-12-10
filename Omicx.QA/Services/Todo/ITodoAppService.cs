using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Services.Todo.Dto;
using Omicx.QA.Services.Todo.Request;
using Volo.Abp.Application.Services;

namespace Omicx.QA.Services.Todo;

public interface ITodoAppService : IApplicationService
{
    Task<string> HelloWorld();
    Task<List<TodoItemDto>> GetListAsync();
    Task<Elasticsearch.Responses.ISearchResponse<TodoItemDocument>> GetListDynamic(FilterTodoDynamicRequest input);
    Task<TodoItemDto> CreateAsync(TodoItemDto todo);
    Task DeleteAsync(Guid id);
}