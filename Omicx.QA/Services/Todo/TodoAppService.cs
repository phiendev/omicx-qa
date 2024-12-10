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
using Omicx.QA.Services.Todo.Service;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Omicx.QA.Services.Todo;

[Route("api/app/todo")]
public class TodoAppService : ApplicationService, ITodoAppService
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly IElasticClient _elasticClient;
    private readonly IRepository<TodoItem, Guid> _todoItemRepository;
    private readonly Task<int?> _customTenantId;
    
    public TodoAppService(
        ICurrentCustomTenant currentCustomTenant,
        IElasticClient elasticClient,
        IRepository<TodoItem, Guid> todoItemRepository
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _elasticClient = elasticClient;
        _todoItemRepository = todoItemRepository;
        _customTenantId = _currentCustomTenant.GetCustomTenantIdAsync();
    }
    
    [HttpGet("hello-world")]
    public async Task<string> HelloWorld()
    {
        return await Task.FromResult("Hello World");
    }
    
    [HttpGet("get-list")]
    public async Task<List<TodoItemDto>> GetListAsync()
    {
        try
        {
            var items = await _todoItemRepository.GetListAsync();
            return ObjectMapper.Map<List<TodoItem>, List<TodoItemDto>>(items);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get list");
            throw new Exception("Failed to get list");
        }
    }
    
    [HttpPost("get-list-dynamic")]
    public async Task<Elasticsearch.Responses.ISearchResponse<TodoItemDocument>> GetListDynamic(FilterTodoDynamicRequest input)
    {
        try
        {
            var fil = FilterFactory.Filter(input.Payload);

            if (fil is null)
                return await Task.FromResult(new Elasticsearch.Responses.SearchResponse<TodoItemDocument>());

            int? customTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();

            var req = PageRequest<TodoItemDocument>
                .Where(fil)
                .ForTenant(customTenantId)
                .Paging(input.CurrentPage, input.PageSize);

            var todoItemDocument = await _elasticClient.QueryAsync<TodoItemDocument>(req);

            return todoItemDocument;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get list dynamic");
            throw new Exception("Failed to get list");
        }
    }
    
    [HttpPost("create-todo")]
    public async Task<TodoItemDto> CreateAsync(TodoItemDto todo)
    {
        try
        {
            var add = ObjectMapper.Map<TodoItemDto, TodoItem>(todo);
            
            if (await _customTenantId is not null)
            {
                add.CustomTenantId =  await _customTenantId;
            }
            
            var result = await _todoItemRepository.InsertAsync(add, autoSave: true);

            if (result is null) throw new Exception("Failed to create todo");
            
            var todoItemDocument = ObjectMapper.Map<TodoItem, TodoItemDocument>(result);
            
            await TodoElasticService.UpsertTodoItem(_elasticClient, todoItemDocument);
            
            return ObjectMapper.Map<TodoItem, TodoItemDto>(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create todo");
            throw new Exception("Failed to create todo");
        }
    }
    
    [HttpDelete("delete-todo")]
    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var todo = await _todoItemRepository.FindAsync(x => x.Id == id);
            if(todo == null) throw new Exception("Failed to delete todo");
            await _todoItemRepository.DeleteAsync(todo, autoSave: true);

            int? customTenantId = await _customTenantId;
            
            await TodoElasticService.DeleteTodoItem(_elasticClient, customTenantId, id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete todo");
            throw new Exception("Failed to delete todo");
        }
    }
}