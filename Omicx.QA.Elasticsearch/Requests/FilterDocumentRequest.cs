using Omicx.QA.Elasticsearch.Filters;

namespace Omicx.QA.Elasticsearch.Requests;

public class FilterDocumentRequest
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public Payload Payload { get; set; }
}