using System.Collections.Generic;


// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

public sealed class SafeDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly object _padlock = new object();

    public int Count
    {
        get
        {
            lock (_padlock)
            {
                return _dictionary.Count;
            }
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_padlock)
            {
                return _dictionary[key];
            }
        }
        set
        {
            lock (_padlock)
            {
                _dictionary[key] = value;
            }
        }
    }

    public SafeDictionary(int capacity)
    {
        _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public SafeDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_padlock)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    public void Add(TKey key, TValue value)
    {
        lock (_padlock)
        {
            if (_dictionary.ContainsKey(key) == false)
            {
                _dictionary.Add(key, value);
            }
        }
    }
}
