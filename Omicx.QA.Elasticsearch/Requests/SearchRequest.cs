using Omicx.QA.Elasticsearch.Expressions;

namespace Omicx.QA.Elasticsearch.Requests;

public class SearchRequest : SearchRequest<SearchRequest>
{
}

public class SearchRequest<T> : ISearchRequest<T> where T : SearchRequest<T>
{
    public int? TenantId { get; set; }
    public IExpression Expression { get; private set; }

    protected T Instance;

    protected SearchRequest()
    {
    }

    public T ForTenant(int? tenantId)
    {
        TenantId = tenantId;

        return Instance;
    }

    public T And(IExpression and, params IExpression[] andExpressions)
    {
        Expression = andExpressions.Length == 0
            ? new AndExpression(Expression, and)
            : new AndExpression(new List<IExpression>(andExpressions) { and }.ToArray());

        return Instance;
    }

    public T Or(IExpression or, params IExpression[] orExpressions)
    {
        Expression = orExpressions.Length == 0
            ? new OrExpression(Expression, or)
            : new OrExpression(new List<IExpression>(orExpressions) { or }.ToArray());

        return Instance;
    }

    public T AndNot(IExpression andNot, params IExpression[] andNotExpressions)
    {
        Expression = andNotExpressions.Length == 0
            ? new AndExpression(Expression, new NotExpression(andNot))
            : new AndExpression(
                Expression,
                new NotExpression(new List<IExpression>(andNotExpressions) { andNot }.ToArray())
            );

        return Instance;
    }

    public T OrNot(IExpression orNot, params IExpression[] orNotExpressions)
    {
        Expression = orNotExpressions.Length == 0
            ? new OrExpression(Expression, new NotExpression(orNot))
            : new OrExpression(
                Expression,
                new NotExpression(new List<IExpression>(orNotExpressions) { orNot }.ToArray())
            );

        return Instance;
    }

    public static T Where(IExpression expression, params IExpression[] expressions)
    {
        if (expression == null) throw new ArgumentNullException(nameof(expression));

        var searchRequest = Empty();
        if (expressions.Length > 0)
        {
            searchRequest.Instance.Expression = new AndExpression(
                new List<IExpression>(expressions) { expression }.ToArray()
            );

            return searchRequest;
        }

        searchRequest.Instance.Expression = expression;

        return searchRequest;
    }

    public static T Empty()
    {
        var searchRequest = Activator.CreateInstance<T>();
        searchRequest.Instance = searchRequest;

        return searchRequest;
    }
}