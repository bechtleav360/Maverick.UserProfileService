using System.Runtime.Serialization;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains sharding methods used for new collections in a database (cluster only).
/// </summary>
public enum AShardingMethod
{
    /// <summary>
    ///     No specific sharding method.
    /// </summary>
    [EnumMember(Value = "")]
    None,

    /// <summary>
    ///     Flexible sharding method.
    /// </summary>
    [EnumMember(Value = "flexible")]
    Flexible,

    /// <summary>
    ///     Single sharding method.
    /// </summary>
    [EnumMember(Value = "single")]
    Single
}
