using System.Collections.Concurrent;

namespace Omicx.QA.Engine;

public class SharedDataCollection
{
    private readonly ConcurrentDictionary<string, object> _inner = new();

    public bool Contains(string key)
    {
        return _inner.ContainsKey(key);
    }

    public bool Set(string key, object value)
    {
        return !Contains(key) && _inner.TryAdd(key, value);
    }

    public bool SetOrUpdate(string key, object value)
    {
        if (!Contains(key)) return _inner.TryAdd(key, value);
        _inner[key] = value;
        return true;
    }

    public T Get<T>(string key)
    {
        if (!Contains(key)) return default;

        if (_inner.TryGetValue(key, out var value)) return (T) value;

        return default;
    }

    public void Clear()
    {
        _inner.Clear();
    }
}