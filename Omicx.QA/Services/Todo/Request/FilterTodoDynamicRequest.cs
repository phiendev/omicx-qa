using Omicx.QA.Elasticsearch.Filters;

namespace Omicx.QA.Services.Todo.Request;

public class FilterTodoDynamicRequest
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public Payload Payload { get; set; }
}