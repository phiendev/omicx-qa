using System.Linq.Dynamic.Core;
using Newtonsoft.Json.Linq;
using Omicx.QA.Elasticsearch.Enums;
using Omicx.QA.Elasticsearch.Expressions;
using Omicx.QA.Elasticsearch.Filters;

namespace Omicx.QA.Elasticsearch.Factories;

public interface IOperatorFilterFactory
{
    IExpression GetFilter(string field, object value, DataType dataType);
}

public abstract class OperatorFilterFactory : IOperatorFilterFactory
{
    public abstract IExpression GetFilter(string field, object value, DataType dataType);

    protected T[] Array<T>(object value)
    {
        if (value == null) return null;

        if (value is Array arr)
        {
            var lst = arr.ToDynamicList();

            return lst.Select(n => (T)n).ToArray();
        }

        if (value is JArray jArray)
        {
            return jArray.Select(token => token.Value<T>()).ToArray();
        }

        if (value is T t) return new[] { t };

        return null;
    }

    protected T Single<T>(object value, int index = 0) where T : struct
    {
        if (value == null) return default;

        if (value is Array arr && arr.Length > index) return Single<T>(arr.GetValue(index));

        if (value is JArray jArr && jArr.Count > index) return jArr[index].Value<T>();

        if (value is T t) return t;

        if (value is JValue jt) return jt.Value<T>();

        return (T)Convert.ChangeType(value, typeof(T));
    }
}

public class TermFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        if (value != null)
        {
            if (dataType == DataType.DateTime) return DateFilter.Eq(field, Convert.ToDateTime(value));
            if (dataType == DataType.Number) return NumberFilter.In(field, Convert.ToDouble(value));
            if (dataType == DataType.String) field = $"{field}.raw";
        }

        return new TermFilter(field, value);
    }
}

public sealed class NotEqualFilterFactory : TermFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
        => new NotExpression(base.GetFilter(field, value, dataType));
}

public class TermsFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        if (value != null)
        {
            if (dataType == DataType.DateTime) return DateFilter.In(field, Array<DateTime>(value));
            if (dataType == DataType.Number) return NumberFilter.In(field, Array<double>(value));
            if (dataType == DataType.String) field = $"{field}.raw";
        }

        return new TermsFilter(field, Array<object>(value));
    }
}

public sealed class NotInFilterFactory : TermsFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
        => new NotExpression(base.GetFilter(field, value, dataType));
}

public sealed class MatchFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(
        string field, object value, DataType dataType) => new MatchFilter(field, (string)value);
}

public sealed class MultiMatchFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        var fields = field.Split(',');

        return new MultiMatchFilter((string)value, fields);
    }
}

public sealed class RangeFilterFactory : OperatorFilterFactory
{
    private readonly Operator _operator;

    public RangeFilterFactory(Operator @operator)
    {
        _operator = @operator;
    }

    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        if (value != null)
        {
            if (dataType == DataType.DateTime)
            {
                if (_operator == Operator.Gt) return DateFilter.Gt(field, Single<DateTime>(value));
                if (_operator == Operator.Gte) return DateFilter.Gte(field, Single<DateTime>(value));
                if (_operator == Operator.Lt) return DateFilter.Lt(field, Single<DateTime>(value));
                if (_operator == Operator.Lte) return DateFilter.Lte(field, Single<DateTime>(value));
                if (_operator == Operator.Between)
                    return DateFilter.Between(field, Single<DateTime>(value), Single<DateTime>(value, 1));
            }

            if (dataType == DataType.Number)
            {
                if (_operator == Operator.Gt) return NumberFilter.Gt(field, Single<double>(value));
                if (_operator == Operator.Gte) return NumberFilter.Gte(field, Single<double>(value));
                if (_operator == Operator.Lt) return NumberFilter.Lt(field, Single<double>(value));
                if (_operator == Operator.Lte) return NumberFilter.Lte(field, Single<double>(value));
                if (_operator == Operator.Between)
                    return NumberFilter.Between(field, Single<double>(value), Single<double>(value, 1));
            }
        }

        throw new NotSupportedException($"Data type {dataType} not support operator {_operator}");
    }
}

public sealed class NullFilterFactory : NotNullFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        return new NotExpression(base.GetFilter(field, value, dataType));
    }
}

public class NotNullFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        return new NotNullFilter(field);
    }
}

public sealed class RegexFilterFactory : OperatorFilterFactory
{
    public override IExpression GetFilter(string field, object value, DataType dataType)
    {
        return new RegexFilter($"{field}.raw", $"(.*)({value})(.*)");
    }
}