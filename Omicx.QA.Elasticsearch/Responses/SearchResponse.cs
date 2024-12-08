using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.Elasticsearch.Responses;

public class SearchResponse<TDoc> : SearchResponse<TDoc, SearchResponse<TDoc>> where TDoc : IElasticNestedEntity
{
    public SearchResponse()
    {
    }

    protected SearchResponse(ICollection<TDoc> documents, long total = 0) : base(documents, total)
    {
    }
}

public class SearchResponse<TDoc, TOut> : ISearchResponse<TDoc>
    where TDoc : IElasticNestedEntity
    where TOut : SearchResponse<TDoc, TOut>
{
    public long Total { get; private set; }
    public string ScrollId { get; private set; }

    public IEnumerable<TDoc> Documents => _documents;

    private readonly List<TDoc> _documents = new List<TDoc>();

    protected TOut Instance;

    protected SearchResponse()
    {
    }

    protected SearchResponse(ICollection<TDoc> documents, long total = 0)
    {
        if (documents != null && documents.Any()) _documents.AddRange(documents);

        Total = total;
    }

    public static TOut Of(ICollection<TDoc> documents)
    {
        var response = Create();
        if (documents != null && documents.Any()) response.Instance._documents.AddRange(documents);

        return response;
    }

    public static TOut Of(long total)
    {
        var response = Create();
        response.Instance.Total = total;

        return response;
    }

    public static TOut Create()
    {
        var searchResponse = Activator.CreateInstance<TOut>();
        searchResponse.Instance = searchResponse;

        return searchResponse;
    }

    public TOut Count(long total = 0)
    {
        Instance.Total = total;

        return Instance;
    }

    public TOut Scroll(string scrollId)
    {
        Instance.ScrollId = scrollId;

        return Instance;
    }

    public TOut Add(TDoc doc)
    {
        if (doc != null) Instance._documents.Add(doc);

        return Instance;
    }
}