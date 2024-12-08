using Nest;
using System.Collections;

namespace Omicx.QA.Elasticsearch.Documents;

[Serializable]
public abstract class NestedAttributes : IReadOnlyDictionary<string, object>
{
    private readonly IDictionary<string, object> _pairs;

    public NestedAttributes()
    {
        _pairs = new Dictionary<string, object>();
    }

    public NestedAttributes(IDictionary<string, object> pairs) : this()
    {
        _pairs = pairs;
    }

    public void Copy(IReadOnlyDictionary<string, object> input)
    {
        if (input == null) return;

        _pairs.Clear();
        foreach (var (key, value) in input) _pairs.Add(key, value);
    }

    [Ignore] //
    public object this[string key]
    {
        get => _pairs.ContainsKey(key) ? _pairs[key] : default;
        set
        {
            if (_pairs.ContainsKey(key)) _pairs.Remove(key);

            _pairs.Add(key, value);
        }
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _pairs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Ignore] //
    public int Count => _pairs.Count;

    [Ignore] //
    public IEnumerable<string> Keys => _pairs.Keys;

    [Ignore] //
    public IEnumerable<object> Values => _pairs.Values;

    public bool ContainsKey(string key) => _pairs.ContainsKey(key);

    public bool TryGetValue(string key, out object value) => _pairs.TryGetValue(key, out value);

    public void Remove(string key)
    {
        _pairs.Remove(key);
    }

    public void Add(string key, object value)
    {
        if (ContainsKey(key)) Remove(key);

        _pairs.TryAdd(key, value);
    }
}