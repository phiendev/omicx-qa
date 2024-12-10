using Nest;
using System.Collections;

namespace Omicx.QA.Elasticsearch.Documents;

[Serializable]
public abstract class NestedAttributes :
    IReadOnlyDictionary<string, object>,
    IEnumerable<KeyValuePair<string, object>>,
    IEnumerable,
    IReadOnlyCollection<KeyValuePair<string, object>>
{
    private readonly IDictionary<string, object> _pairs;

    public NestedAttributes()
    {
        this._pairs = (IDictionary<string, object>)new Dictionary<string, object>();
    }

    public NestedAttributes(IDictionary<string, object> pairs)
        : this()
    {
        this._pairs = pairs;
    }

    public void Copy(IReadOnlyDictionary<string, object> input)
    {
        if (input == null)
            return;
        this._pairs.Clear();
        foreach ((string key, object obj) in (IEnumerable<KeyValuePair<string, object>>)input)
            this._pairs.Add(key, obj);
    }

    [Ignore]
    public object this[string key]
    {
        get => !this._pairs.ContainsKey(key) ? (object)null : this._pairs[key];
        set
        {
            if (this._pairs.ContainsKey(key))
                this._pairs.Remove(key);
            this._pairs.Add(key, value);
        }
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => this._pairs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

    [Ignore] public int Count => this._pairs.Count;

    [Ignore] public IEnumerable<string> Keys => (IEnumerable<string>)this._pairs.Keys;

    [Ignore] public IEnumerable<object> Values => (IEnumerable<object>)this._pairs.Values;

    public bool ContainsKey(string key) => this._pairs.ContainsKey(key);

    public bool TryGetValue(string key, out object value)
    {
        return this._pairs.TryGetValue(key, out value);
    }

    public void Remove(string key) => this._pairs.Remove(key);

    public void Add(string key, object value)
    {
        if (this.ContainsKey(key))
            this.Remove(key);
        this._pairs.TryAdd<string, object>(key, value);
    }
}