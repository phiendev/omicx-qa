namespace Omicx.QA.Elasticsearch.Extensions;

public interface IDynamicAttribute
{
    (string, object) GetProperty();
}