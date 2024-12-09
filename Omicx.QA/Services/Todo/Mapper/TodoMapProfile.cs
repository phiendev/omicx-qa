using AutoMapper;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Entities.Todo;
using Omicx.QA.Services.Todo.Dto;

namespace Omicx.QA.Services.Todo.Mapper;

public class TodoMapProfile : Profile
{
    public TodoMapProfile()
    {
        CreateMap<TodoItem, TodoItemDto>().ReverseMap();
        CreateMap<TodoItem, TodoItemDocument>().ReverseMap();
        CreateMap<TodoItemDto, TodoItemDocument>().ReverseMap();
    }
}