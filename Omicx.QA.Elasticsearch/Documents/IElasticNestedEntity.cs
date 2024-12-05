namespace Omicx.QA.Elasticsearch.Documents;

public interface IElasticNestedEntity : IReadOnlyDictionary<string, object>
{
    void Add(string key, object value);

    void Remove(string key);
}