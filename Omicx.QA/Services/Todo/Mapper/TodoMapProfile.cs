using AutoMapper;
using Omicx.QA.Entities;
using Omicx.QA.Entities.Todo;
using Omicx.QA.Services.Todo.Dto;

namespace Omicx.QA.Services.Todo.Mapper;

public class TodoMapProfile : Profile
{
    public TodoMapProfile()
    {
        CreateMap<TodoItem, TodoItemDto>();
        CreateMap<TodoItemDto, TodoItem>();
    }
}