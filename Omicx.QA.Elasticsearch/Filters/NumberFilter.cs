using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class NumberFilter : Filter<double>
{
    private double? _eq;
    private double? _lt;
    private double? _lte;
    private double? _gt;
    private double? _gte;
    private double[] _in;
    private double[] _between;

    public NumberFilter(string field, params double[] values) : base(field, values)
    {
    }

    public NumberFilter(string field, double value) : base(field, value)
    {
    }

    public static NumberFilter Eq(string field, double d) => new NumberFilter(field, d) { _eq = d };

    public static NumberFilter In(string field, params double[] doubles) =>
        new NumberFilter(field, doubles) { _in = doubles };

    public static NumberFilter Lt(string field, double d) => new NumberFilter(field, d) { _lt = d };
    public static NumberFilter Lte(string field, double d) => new NumberFilter(field, d) { _lte = d };
    public static NumberFilter Gt(string field, double d) => new NumberFilter(field, d) { _gt = d };
    public static NumberFilter Gte(string field, double d) => new NumberFilter(field, d) { _gte = d };

    public static NumberFilter Between(string field, double d1, double d2)
    {
        var dateFilter = new NumberFilter(field, d1, d2) { _between = new[] { d1, d2 } };
        Array.Sort(dateFilter._between);

        return dateFilter;
    }

    public override QueryContainer GetQuery(string prefix = null)
    {
        if (_eq != null) return EqQuery(prefix);

        if (_in != null) return InQuery(prefix);

        if (_lt != null) return LtQuery(prefix);

        if (_lte != null) return LteQuery(prefix);

        if (_gt != null) return GtQuery(prefix);

        if (_gte != null) return GteQuery(prefix);

        if (_between is { Length: 2 }) return BetweenQuery(prefix);

        throw new NotSupportedException("Not support number filter");
    }

    private NumericRangeQuery LtQuery(string prefix = null) => new NumericRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        LessThan = _lt
    };

    private NumericRangeQuery LteQuery(string prefix = null) => new NumericRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        LessThanOrEqualTo = _lte
    };

    private NumericRangeQuery GtQuery(string prefix = null) => new NumericRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        GreaterThan = _gt
    };

    private NumericRangeQuery GteQuery(string prefix = null) => new NumericRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        GreaterThanOrEqualTo = _gte
    };

    private QueryContainer EqQuery(string prefix = null) => new TermQuery
    {
        Field = FieldIncludePrefix(prefix),
        Value = _eq
    };

    private NumericRangeQuery BetweenQuery(string prefix = null)
    {
        return new NumericRangeQuery
        {
            Field = FieldIncludePrefix(prefix),
            LessThanOrEqualTo = _between[1],
            GreaterThanOrEqualTo = _between[0]
        };
    }

    private QueryContainer InQuery(string prefix = null)
    {
        var doubles = _in?.Select(x => (object)x) ?? Enumerable.Empty<object>();
        return new TermsQuery { Field = FieldIncludePrefix(prefix), Terms = doubles };
    }
}