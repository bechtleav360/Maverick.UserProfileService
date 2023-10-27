using System.Runtime.Serialization;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains ArangoDB sharding strategies.
/// </summary>
public enum AShardingStrategy
{
    /// <summary>
    ///     default sharding used for new collections starting from version 3.4 (excluding smart edge collections)
    /// </summary>
    [EnumMember(Value = "hash")]
    Hash,

    /// <summary>
    ///     default sharding used by ArangoDB Community Edition before version 3.4
    /// </summary>
    [EnumMember(Value = "community-compat")]
    CommunityCompat,

    /// <summary>
    ///     default sharding used by ArangoDB Enterprise Edition before version 3.4
    /// </summary>
    [EnumMember(Value = "enterprise-compat")]
    EnterpriseCompat,

    /// <summary>
    ///     default sharding used by smart edge collections in ArangoDB Enterprise Edition before version 3.4
    /// </summary>
    [EnumMember(Value = "enterprise-smart-edge-compat")]
    EnterpriseSmartEdgeCompat,

    /// <summary>
    ///     default sharding used for new smart edge collections starting from version 3.4
    /// </summary>
    [EnumMember(Value = "enterprise-hash-smart-edge")]
    EnterpriseSmartEdgeHash
}
