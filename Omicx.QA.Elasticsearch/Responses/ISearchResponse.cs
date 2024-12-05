using Newtonsoft.Json;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.Elasticsearch.Responses;

public interface ISearchResponse<out TDoc> : IElasticResponse where TDoc : IElasticNestedEntity
{
    [JsonIgnore]
    string ScrollId { get; }
    long Total { get; }

    IEnumerable<TDoc> Documents { get; }
}