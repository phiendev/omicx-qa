using Nest;

namespace Omicx.QA.Elasticsearch.Filters
{
    public class NotNullFilter : Filter<object>
    {
        public NotNullFilter(string field) : base(field, values: new object[] {true})
        {
        }

        public override QueryContainer GetQuery(string prefix = null)
        {
            return new ExistsQuery {Field = FieldIncludePrefix(prefix)};
        }
    }
}