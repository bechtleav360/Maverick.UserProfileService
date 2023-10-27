using System.Runtime.Serialization;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains sharding methods used for new collections in a database (cluster only).
/// </summary>
public enum AShardingMethod
{
    [EnumMember(Value = "")]
    None,

    [EnumMember(Value = "flexible")]
    Flexible,

    [EnumMember(Value = "single")]
    Single
}
