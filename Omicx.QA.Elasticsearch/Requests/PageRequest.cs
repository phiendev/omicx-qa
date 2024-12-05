using System.Linq.Expressions;
using System.Reflection;
using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.Elasticsearch.Requests;

public class PageRequest<TDoc> : PageRequest<TDoc, PageRequest<TDoc>> where TDoc : IElasticNestedEntity
{
}

public class PageRequest<TDoc, T>
    : SearchRequest<T>, IPageRequest where T : PageRequest<TDoc, T> where TDoc : IElasticNestedEntity
{
    private int PageSize { get; set; } = 10;
    private int CurrentPage { get; set; } = 1;
    public int Skip => (CurrentPage - 1) * PageSize;
    public int Take => PageSize;
    private int? SearchAfterSize { get; set; }
    public int? Size => SearchAfterSize;
    public ICollection<string> OrderBy => _asc;
    public ICollection<string> OrderByDesc => _desc;
    public string ScrollTime { get; private set; }
    public IList<string> ListSearchAfter => _search_after;
    private readonly IList<string> _asc = new List<string>();
    private readonly IList<string> _desc = new List<string>();
    private readonly IList<string> _search_after = new List<string>();

    private readonly
        List<Func<SortDescriptor<IDictionary<string, object>>, SortDescriptor<IDictionary<string, object>>>>
        _sortingExpressions =
            new List<Func<SortDescriptor<IDictionary<string, object>>,
                SortDescriptor<IDictionary<string, object>>>>();

    public ICollection<Func<SortDescriptor<IDictionary<string, object>>,
        SortDescriptor<IDictionary<string, object>>>> SortingExpressions =>
        _sortingExpressions;

    protected PageRequest()
    {
    }

    public T Paging(int currentPage = 1, int pageSize = 10)
    {
        if (currentPage < 1)
            throw new ArgumentOutOfRangeException(nameof(currentPage), "Must be greater than or equals 1");

        if (pageSize < 1 || pageSize > 1000)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Must be greater than or equals 1 and less than or equals 1000"
            );

        Instance.CurrentPage = currentPage;
        Instance.PageSize = pageSize;

        return Instance;
    }

    public T Scroll(string scrollTime)
    {
        Instance.ScrollTime = scrollTime;

        return Instance;
    }

    public T Next()
    {
        Instance.CurrentPage++;

        return Instance;
    }

    public T Asc(Expression<Func<TDoc, object>> orderBy, Type propType = null)
    {
        if (orderBy == null) return Instance;

        var (returnType, name) = GetFieldName(orderBy);
        if (propType != null) returnType = propType;

        Instance._asc.Add(typeof(string) == returnType ? $"{name}.raw" : name);

        return Instance;
    }

    public T Desc(Expression<Func<TDoc, object>> orderBy, Type propType = null)
    {
        if (orderBy == null) return Instance;

        var (returnType, name) = GetFieldName(orderBy);
        if (propType != null) returnType = propType;

        Instance._desc.Add(typeof(string) == returnType ? $"{name}.raw" : name);

        return Instance;
    }

    public T Descending(Expression<Func<TDoc, object>> orderBy, Type propType = null)
    {
        if (orderBy == null) return Instance;

        var (returnType, name) = GetFieldName(orderBy);
        if (propType != null) returnType = propType;

        Instance._sortingExpressions.Add(descriptor => descriptor
            .Descending(typeof(string) == returnType ? $"{name.ToCamelCase()}.raw" : name.ToCamelCase())
        );

        return Instance;
    }

    public T Script(string script, string type = "string", SortOrder sortOrder = SortOrder.Ascending)
    {
        Instance._sortingExpressions.Add(descriptor => descriptor.Script(ssd => ssd
            .Type(type)
            .Script(ss => ss.Source(script).Lang("painless"))
            .Order(sortOrder)
        ));

        return Instance;
    }

    public T SearchAfter(List<string> searchAfterList)
    {
        if (searchAfterList == null || !searchAfterList.Any()) return Instance;
        searchAfterList.ForEach(w => { Instance._search_after.Add(w); });
        return Instance;
    }

    public T Sizing(int? size)
    {
        if (size == null) return Instance;
        Instance.SearchAfterSize = size;
        return Instance;
    }

    private static (Type, string) GetFieldName(Expression<Func<TDoc, object>> exp)
    {
        if (exp.Body is MethodCallExpression mcExp)
        {
            var lastArg = mcExp.Arguments.LastOrDefault();
            if (lastArg == null) return (null, null);

            var constantExp = (ConstantExpression)lastArg;

            return (constantExp.Type, (string)constantExp.Value);
        }

        var memberExpression = exp.Body is UnaryExpression unaryExp
            ? (MemberExpression)unaryExp.Operand
            : (MemberExpression)exp.Body;

        var propInfo = (PropertyInfo)memberExpression.Member;

        return (propInfo.GetMethod?.ReturnType, propInfo.Name);
    }
}