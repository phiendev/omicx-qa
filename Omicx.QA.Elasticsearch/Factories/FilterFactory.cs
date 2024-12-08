using Omicx.QA.Elasticsearch.Enums;
using Omicx.QA.Elasticsearch.Expressions;
using Omicx.QA.Elasticsearch.Filters;

namespace Omicx.QA.Elasticsearch.Factories;

public static class FilterFactory
{
    private static readonly IDictionary<Operator, IOperatorFilterFactory> Filters;

    static FilterFactory()
    {
        Filters = new Dictionary<Operator, IOperatorFilterFactory>
        {
            { Operator.Eq, new TermFilterFactory() },
            { Operator.In, new TermsFilterFactory() },
            { Operator.Match, new MatchFilterFactory() },
            { Operator.Ne, new NotEqualFilterFactory() },
            { Operator.Nin, new NotInFilterFactory() },
            { Operator.MultiMatch, new MultiMatchFilterFactory() },
            { Operator.Lt, new RangeFilterFactory(Operator.Lt) },
            { Operator.Lte, new RangeFilterFactory(Operator.Lte) },
            { Operator.Gt, new RangeFilterFactory(Operator.Gt) },
            { Operator.Gte, new RangeFilterFactory(Operator.Gte) },
            { Operator.Between, new RangeFilterFactory(Operator.Between) },
            { Operator.Contain, new RegexFilterFactory() },
            { Operator.Null, new NullFilterFactory() },
            { Operator.NotNull, new NotNullFilterFactory() },
        };
    }

    public static IOperatorFilterFactory GetFactory(Operator @operator)
    {
        if (Filters.ContainsKey(@operator))
        {
            return Filters[@operator];
        }

        throw new NotSupportedException($"{@operator} has not supported yet");
    }

    public static IExpression Filter(IPayload payload)
    {
        if (payload is SinglePayload sp)
            return GetFactory(sp.Operator).GetFilter(sp.Field, sp.Value, sp.DataType);

        if (payload is ArrayPayload ap)
        {
            var filters = ap.Select(Filter).Where(f => f != null).ToArray();
            if (filters.Length == 0) return null;

            if (payload.Type == FilterType.And) return new AndExpression(filters);
            if (payload.Type == FilterType.Or) return new OrExpression(filters);
            if (payload.Type == FilterType.Not) return new NotExpression(filters);
        }

        var payloadData = payload?.GetPayloadData();

        return payloadData != null ? Filter(payloadData) : null;
    }
}