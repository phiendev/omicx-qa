using Nest;

namespace Omicx.QA.Elasticsearch.Expressions;

public abstract class Expression : IExpression
{
    protected IExpression[] Expressions => _expressions.ToArray();

    private readonly List<IExpression> _expressions = new List<IExpression>();

    protected Expression()
    {
    }

    protected Expression(params IExpression[] expressions) : this()
    {
        if (expressions.Length == 0) throw new ArgumentException("Require at least 1 expression");

        _expressions.AddRange(expressions);
    }

    protected void AddExpression(IExpression expression)
    {
        _expressions.Add(expression);
    }

    public IExpression And(params IExpression[] expressions) =>
        new AndExpression(new List<IExpression>(expressions) {this}.ToArray());

    public IExpression Or(params IExpression[] expressions) =>
        new OrExpression(new List<IExpression>(expressions) {this}.ToArray());

    public IExpression AndNot(params IExpression[] expressions) =>
        new AndExpression(this, new NotExpression(expressions));

    public IExpression OrNot(params IExpression[] expressions) =>
        new OrExpression(this, new NotExpression(expressions));

    public abstract QueryContainer GetQuery(string prefix = null);
}