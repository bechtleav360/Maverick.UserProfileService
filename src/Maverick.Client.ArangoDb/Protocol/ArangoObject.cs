namespace Maverick.Client.ArangoDb.Protocol;

/// <summary>
///     Object that will be used for queries and contains a data object and ArangoDB internal properties of each document.
/// </summary>
public class ArangoObject
{
    /// <summary>
    ///     Gets or sets the data associated with the object.
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    ///     Gets an empty instance of <see cref="ArangoObject"/>.
    /// </summary>
    public static ArangoObject Empty => new ArangoObject();

    /// <summary>
    ///     Gets the internal ID of the object.
    /// </summary>
    public string InternalId { get; }

    /// <summary>
    ///     Gets a value indicating whether the object is valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(InternalId) && !string.IsNullOrEmpty(Key) && Data != null;

    /// <summary>
    ///     Gets the key associated with the object.
    /// </summary>
    public string Key { get; }

    // Private constructor to prevent external instantiation of an empty ArangoObject.
    private ArangoObject()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoObject"/> class with the specified internal ID, key, and data.
    /// </summary>
    /// <param name="internalId">The internal ID of the object.</param>
    /// <param name="key">The key associated with the object.</param>
    /// <param name="data">The data associated with the object.</param>
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
    /// <summary>
    ///     Gets the data stored in the ArangoObject.
    /// </summary>
    public T Data { get; }

    /// <summary>
    ///     Gets the internal ID associated with the ArangoObject.
    /// </summary>
    public string InternalId { get; }

    /// <summary>
    ///     Gets a value indicating whether the ArangoObject is valid.
    /// </summary>
    /// <remarks>
    ///     An ArangoObject is considered valid if its internal ID, key, and data are not null or empty.
    /// </remarks>
    public bool IsValid => !string.IsNullOrEmpty(InternalId) && !string.IsNullOrEmpty(Key) && Data != null;

    /// <summary>
    ///     Gets the key associated with the ArangoObject.
    /// </summary>
    public string Key { get; }

    // Private constructor for creating an empty ArangoObject.
    private ArangoObject()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoObject{T}"/> class.
    /// </summary>
    /// <param name="internalId">The internal ID of the ArangoObject.</param>
    /// <param name="key">The key associated with the ArangoObject.</param>
    /// <param name="data">The data to be stored in the ArangoObject.</param>
    public ArangoObject(string internalId, string key, T data)
    {
        InternalId = internalId;
        Key = key;
        Data = data;
    }
}
