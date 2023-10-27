namespace Maverick.Client.ArangoDb.Protocol;

/// <summary>
///     Object that will be used for queries and contains a data object and ArangoDB internal properties of each document.
/// </summary>
public class ArangoObject
{
    public object Data { get; set; }

    public static ArangoObject Empty => new ArangoObject();
    public string InternalId { get; }

    public bool IsValid => !string.IsNullOrEmpty(InternalId) && !string.IsNullOrEmpty(Key) && Data != null;
    public string Key { get; }

    private ArangoObject()
    {
    }

    public ArangoObject(string internalId, string key, object data)
    {
        InternalId = internalId;
        Key = key;
        Data = data;
    }
}

/// <summary>
///     Object that will be used for queries and contains a
///     data object and ArangoDB internal properties of each document.
/// </summary>
/// <typeparam name="T">Type of the data instance.</typeparam>
public class ArangoObject<T>
{
    public T Data { get; }

    public static ArangoObject<T> Empty => new ArangoObject<T>();
    public string InternalId { get; }

    public bool IsValid => !string.IsNullOrEmpty(InternalId) && !string.IsNullOrEmpty(Key) && Data != null;
    public string Key { get; }

    private ArangoObject()
    {
    }

    public ArangoObject(string internalId, string key, T data)
    {
        InternalId = internalId;
        Key = key;
        Data = data;
    }
}
