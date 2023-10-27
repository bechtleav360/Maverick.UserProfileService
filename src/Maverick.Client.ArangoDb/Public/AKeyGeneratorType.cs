using System.Runtime.Serialization;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains the key generator types
/// </summary>
public enum AKeyGeneratorType
{
    /// <summary>
    ///     Generates numerical keys in ascending order
    /// </summary>
    [EnumMember(Value = "traditional")]
    Traditional,

    /// <summary>
    ///     Generates numerical keys in ascending order,
    ///     the inital offset and the spacing can be configured
    /// </summary>
    [EnumMember(Value = "autoincrement")]
    Autoincrement,

    /// <summary>
    ///     generates keys of a fixed length (16 bytes) in
    ///     ascending lexicographical sort order. This is ideal for usage with the RocksDB
    ///     engine, which will slightly benefit keys that are inserted in lexicographically
    ///     ascending order. The key generator can be used in a single-server or cluster.
    /// </summary>
    [EnumMember(Value = "padded")]
    Padded,

    /// <summary>
    ///     generates universally unique 128 bit keys, which
    ///     are stored in hexadecimal human-readable format. This key generator can be used
    ///     in a single-server or cluster to generate "seemingly random" keys. The keys
    ///     produced by this key generator are not lexicographically sorted.
    /// </summary>
    [EnumMember(Value = "uuid")]
    Uuid
}
