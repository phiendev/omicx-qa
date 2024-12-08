using Nest;

namespace Omicx.QA.Elasticsearch.Requests;

public interface IPageRequest : ISearchRequest<IPageRequest>
{
    int Skip { get; }

    int Take { get; }
    IList<string> ListSearchAfter { get; }
    int? Size { get; }

    ICollection<string> OrderBy { get; }

    ICollection<string> OrderByDesc { get; }

    bool HasSort => (OrderBy != null && OrderBy.Any())
                    || (OrderByDesc != null && OrderByDesc.Any())
                    || (SortingExpressions != null && SortingExpressions.Any());
        
    string ScrollTime { get; }
        
    public ICollection<Func<SortDescriptor<IDictionary<string, object>>,
        SortDescriptor<IDictionary<string, object>>>> SortingExpressions { get; }
}