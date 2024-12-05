using Nest;

namespace Omicx.QA.Elasticsearch.Filters;

public class DateFilter : Filter<DateTime>
{
    private bool _dateOnly;
    private DateTime? _eq;
    private DateTime? _lt;
    private DateTime? _lte;
    private DateTime? _gt;
    private DateTime? _gte;
    private DateTime[] _between;
    private DateTime[] _in;

    public DateFilter(string field, params DateTime[] values) : base(field, values)
    {
    }

    public DateFilter(string field, DateTime value) : base(field, value)
    {
    }

    public static DateFilter Eq(string field, bool? dateOnly, DateTime dt) => new DateFilter(field, dt)
    {
        _eq = dt,
        _dateOnly = dateOnly ?? false
    };

    public static DateFilter Eq(string field, DateTime dt) => Eq(field, false, dt);

    public static DateFilter In(string field, bool? dateOnly = false, params DateTime[] dateTimes) =>
        new DateFilter(field, dateTimes) { _in = dateTimes, _dateOnly = dateOnly ?? false };

    public static DateFilter In(string field, params DateTime[] dateTimes) => In(field, false, dateTimes);

    public static DateFilter Lt(string field, DateTime dt) => new DateFilter(field, dt) { _lt = dt };
    public static DateFilter Lte(string field, DateTime dt) => new DateFilter(field, dt) { _lte = dt };
    public static DateFilter Gt(string field, DateTime dt) => new DateFilter(field, dt) { _gt = dt };
    public static DateFilter Gte(string field, DateTime dt) => new DateFilter(field, dt) { _gte = dt };

    public static DateFilter Between(string field, DateTime dt1, DateTime dt2)
    {
        var dateFilter = new DateFilter(field, dt1, dt2) { _between = new[] { dt1, dt2 } };
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

        if (_between != null && (_between.Length == 1 || _between.Length == 2)) return BetweenQuery(prefix);

        throw new NotSupportedException("Not support date filter");
    }

    private DateRangeQuery LtQuery(string prefix = null) => new DateRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        LessThan = DateMath.Anchored(_lt.GetValueOrDefault())
    };

    private DateRangeQuery LteQuery(string prefix = null) => new DateRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        LessThanOrEqualTo = DateMath.Anchored(_lte.GetValueOrDefault())
    };

    private DateRangeQuery GtQuery(string prefix = null) => new DateRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        GreaterThan = DateMath.Anchored(_gt.GetValueOrDefault())
    };

    private DateRangeQuery GteQuery(string prefix = null) => new DateRangeQuery
    {
        Field = FieldIncludePrefix(prefix),
        GreaterThanOrEqualTo = DateMath.Anchored(_gte.GetValueOrDefault())
    };

    private DateRangeQuery BetweenQuery(string prefix = null)
    {
        var start = DateTime.Now;
        var end = DateTime.Now;

        if (_between.Length == 2)
        {
            start = _between[0];
            end = _between[1];
        }
        else
        {
            var root = _between[0];
            if (root.CompareTo(DateTime.Now) > 0) end = root;
            else start = root;
        }

        return new DateRangeQuery
        {
            Field = FieldIncludePrefix(prefix),
            LessThanOrEqualTo = DateMath.Anchored(end),
            GreaterThanOrEqualTo = DateMath.Anchored(start)
        };
    }

    private QueryContainer EqQuery(string prefix = null)
    {
        if (!_dateOnly) return new TermQuery { Field = FieldIncludePrefix(prefix), Value = _eq.GetValueOrDefault() };

        var start = _eq.GetValueOrDefault().Date;
        var end = start.AddDays(1).AddTicks(-1);

        _between = new[] { start, end };

        return BetweenQuery(prefix);
    }

    private QueryContainer InQuery(string prefix = null)
    {
        var dateTimes = _in?.Select(x => (object)x) ?? Enumerable.Empty<object>();
        if (!_dateOnly) return new TermsQuery { Field = FieldIncludePrefix(prefix), Terms = dateTimes };

        var should = _in?.Select(dt => Eq(Field, _dateOnly, dt).GetQuery());

        return new BoolQuery { Should = should };
    }
}