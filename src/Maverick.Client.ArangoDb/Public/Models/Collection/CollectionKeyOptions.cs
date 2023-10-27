using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Object containing the collection key options
/// </summary>
public class CollectionKeyOptions
{
    /// <summary>
    ///     if set to true, then it is allowed to supply own key values in the
    ///     _key attribute of a document.If set to false, then the key generator
    ///     will solely be responsible for generating keys and supplying own key values
    ///     in the _key attribute of documents is considered an error.
    /// </summary>
    public bool AllowUserKeys { get; set; }

    /// <summary>
    ///     increment value for autoincrement key generator. Not used for other key
    ///     generator types.
    /// </summary>
    public long Increment { get; set; }

    /// <summary>
    ///     Initial offset value for autoincrement key generator.
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    ///     specifies the type of the key generator. The currently available generators are
    ///     traditional, autoincrement, uuid and padded.
    ///     The traditional key generator generates numerical keys in ascending order.
    ///     The autoincrement key generator generates numerical keys in ascending order,
    ///     the inital offset and the spacing can be configured
    ///     The padded key generator generates keys of a fixed length (16 bytes) in
    ///     ascending lexicographical sort order.This is ideal for usage with the RocksDB
    ///     engine, which will slightly benefit keys that are inserted in lexicographically
    ///     ascending order.The key generator can be used in a single-server or cluster.
    ///     The uuid key generator generates universally unique 128 bit keys, which
    ///     are stored in hexadecimal human-readable format. This key generator can be used
    ///     in a single-server or cluster to generate "seemingly random" keys.The keys
    ///     produced by this key generator are not lexicographically sorted.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public AKeyGeneratorType Type { get; set; }
}
