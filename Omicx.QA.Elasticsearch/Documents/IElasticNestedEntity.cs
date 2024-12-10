using System.Collections;

namespace Omicx.QA.Elasticsearch.Documents;

public interface IElasticNestedEntity :
    IReadOnlyDictionary<string, object>,
    IEnumerable<KeyValuePair<string, object>>,
    IEnumerable,
    IReadOnlyCollection<KeyValuePair<string, object>>
{
    void Add(string key, object value);

    void Remove(string key);
}