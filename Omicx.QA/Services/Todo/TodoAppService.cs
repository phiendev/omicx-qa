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

[Route("api/app/todo")]
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
    
    [HttpGet("hello-world")]
    public async Task<string> HelloWorld()
    {
        return await Task.FromResult("Hello World");
    }
    
    [HttpGet("get-list")]
    public async Task<List<TodoItemDto>> GetListAsync()
    {
        var items = await _todoItemRepository.GetListAsync();
        return ObjectMapper.Map<List<TodoItem>, List<TodoItemDto>>(items);
    }
    
    [HttpPost("get-list-dynamic")]
    public async Task<Elasticsearch.Responses.ISearchResponse<TodoItemDocument>> GetListDynamic(FilterTodoDynamicRequest input)
    {
        var fil = FilterFactory.Filter(input.Payload);
        
        if (fil == null)
            return await Task.FromResult(new Elasticsearch.Responses.SearchResponse<TodoItemDocument>());
        
        int? customTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
        
        var req = PageRequest<TodoItemDocument>
            .Where(fil)
            .ForTenant(customTenantId)
            .Paging(input.CurrentPage, input.PageSize);
        
        var todoItemDocument = await _elasticClient.QueryAsync<TodoItemDocument>(req);
        
        return todoItemDocument;
    }
    
    [HttpPost("create-todo")]
    public async Task<TodoItemDto> CreateAsync(TodoItemDto todo)
    {
        var add = ObjectMapper.Map<TodoItemDto, TodoItem>(todo);
        
        var result = await _todoItemRepository.InsertAsync(add);

        if (result == null) throw new Exception("Thêm không thành công");

        return ObjectMapper.Map<TodoItem, TodoItemDto>(result);
    }
    
    [HttpDelete("delete-todo")]
    public async Task DeleteAsync(Guid id)
    {
        await _todoItemRepository.DeleteAsync(id);
    }
}