namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contains additional options for key generation
/// </summary>
public class KeyOptions
{
    /// <summary>
    ///     if set to true, then it is allowed to supply own key values in the
    ///     _key attribute of a document.If set to false, then the key generator
    ///     will solely be responsible for generating keys and supplying own key values
    ///     in the _key attribute of documents is considered an error.
    /// </summary>
    public bool AllowUserKeys { get; set; }

    /// <summary>
    ///     Last (key) Value
    /// </summary>
    public int LastValue { get; set; }

    /// <summary>
    ///     specifies the type of the key generator. The currently available generators are
    ///     traditional, autoincrement, uuid and padded.
    ///     The traditional key generator generates numerical keys in ascending order.
    ///     The autoincrement key generator generates numerical keys in ascending order,
    ///     the initial offset and the spacing can be configured
    ///     The padded key generator generates keys of a fixed length (16 bytes) in
    ///     ascending lexicographical sort order.This is ideal for usage with the RocksDB
    ///     engine, which will slightly benefit keys that are inserted in lexicographically
    ///     ascending order.The key generator can be used in a single-server or cluster.
    ///     The uuid key generator generates universally unique 128 bit keys, which
    ///     are stored in hexadecimal human-readable format. This key generator can be used
    ///     in a single-server or cluster to generate "seemingly random" keys.The keys
    ///     produced by this key generator are not lexicographically sorted.
    /// </summary>
    public string Type { get; set; }
}
