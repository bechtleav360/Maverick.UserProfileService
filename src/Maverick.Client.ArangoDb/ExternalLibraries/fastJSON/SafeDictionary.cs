using System.Collections.Generic;


// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

/// <summary>
///     Represents a thread-safe dictionary that ensures safe concurrent access.
/// </summary>
public sealed class SafeDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly object _padlock = new object();

    /// <summary>
    ///     Gets the number of key-value pairs contained in the dictionary.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="SafeDictionary{TKey, TValue}"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    public SafeDictionary(int capacity)
    {
        _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SafeDictionary{TKey, TValue}"/> class with default capacity.
    /// </summary>
    public SafeDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    /// <summary>
    ///     Tries to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns><see langword="true" /> if the dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_padlock)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    /// <summary>
    ///     Adds an element with the provided key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
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
