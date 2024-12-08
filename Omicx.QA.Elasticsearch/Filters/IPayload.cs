using Omicx.QA.Elasticsearch.Enums;

namespace Omicx.QA.Elasticsearch.Filters;

public interface IPayload
{
    public FilterType Type { get; set; }

    IPayload GetPayloadData();
}