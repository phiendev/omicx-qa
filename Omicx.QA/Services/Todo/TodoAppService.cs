using Microsoft.AspNetCore.Mvc;
using Nest;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Elasticsearch.Factories;
using Omicx.QA.Elasticsearch.Requests;
using Omicx.QA.Entities.Todo;
using Omicx.QA.MultiTenancy.Customs;
using Omicx.QA.Services.Todo.Dto;
using Omicx.QA.Services.Todo.Request;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Omicx.QA.Services.Todo;

public class TodoAppService : ApplicationService, ITodoAppService
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly IElasticClient _elasticClient;
    private readonly IRepository<TodoItem, Guid> _todoItemRepository;
    
    public TodoAppService(
        ICurrentCustomTenant currentCustomTenant,
        IElasticClient elasticClient,
        IRepository<TodoItem, Guid> todoItemRepository
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _elasticClient = elasticClient;
        _todoItemRepository = todoItemRepository;
    }
    
    [HttpGet]
    public async Task<string> HelloWorld()
    {
        return await Task.FromResult("Hello World");
    }
    
    [HttpGet]
    public async Task<List<TodoItemDto>> GetListAsync()
    {
        var items = await _todoItemRepository.GetListAsync();
        return ObjectMapper.Map<List<TodoItem>, List<TodoItemDto>>(items);
    }
    
    [HttpPost]
    public async Task<Elasticsearch.Responses.ISearchResponse<TodoItemDocument>> GetTodosDynamic(FilterTodoDynamicRequest input)
    {
        var fil = FilterFactory.Filter(input.Payload);
        
        if (fil == null)
            return await Task.FromResult(new Elasticsearch.Responses.SearchResponse<TodoItemDocument>());
        
        var req = PageRequest<TodoItemDocument>
            .Where(fil)
            // .ForTenant(_currentTenant.CustomTenantId)
            .Desc(x => x.Id)
            .Paging(input.CurrentPage, input.PageSize);
        
        var todoItemDocument = await _elasticClient.QueryAsync<TodoItemDocument>(req);

        return todoItemDocument;
    }
    
    [HttpPost]
    public async Task<TodoItemDto> CreateAsync(TodoItemDto todo)
    {
        var add = ObjectMapper.Map<TodoItemDto, TodoItem>(todo);
        
        var result = await _todoItemRepository.InsertAsync(add);

        if (result == null) throw new Exception("Thêm không thành công");

        return ObjectMapper.Map<TodoItem, TodoItemDto>(result);
    }
    
    [HttpDelete]
    public async Task DeleteAsync(Guid id)
    {
        await _todoItemRepository.DeleteAsync(id);
    }
}