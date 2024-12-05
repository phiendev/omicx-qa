using Microsoft.AspNetCore.Mvc;
using Omicx.QA.Entities.Todo;
using Omicx.QA.Services.Todo.Dto;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Omicx.QA.Services.Todo;

public class TodoAppService : ApplicationService, ITodoAppService
{
    private readonly IRepository<TodoItem, Guid> _todoItemRepository;
    
    public TodoAppService(IRepository<TodoItem, Guid> todoItemRepository)
    {
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