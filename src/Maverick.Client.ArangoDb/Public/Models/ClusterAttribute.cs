namespace Maverick.Client.ArangoDb.Public.Models;

/// <summary>
///     Contains attributes that are only meaningful by cluster operations.
/// </summary>
public class ClusterAttribute
{
    /// <summary>
    ///     contains how many minimal copies of each shard need to be in sync on different DBServers.
    /// </summary>
    public int MinReplicationFactor { get; }

    /// <summary>
    ///     Contains how many copies of each shard are kept on different DBServers.
    /// </summary>
    public int ReplicationFactor { get; }

    /// <summary>
    ///     the sharding strategy selected for the collection.
    /// </summary>
    public string ShardingStrategy { get; }

    /// <summary>
    ///     contains the names of document attributes that are used to
    ///     determine the target shard for documents
    /// </summary>
    public string ShardKeys { get; }

    /// <summary>
    ///     Attribute that is used in smart graphs,
    /// </summary>
    public string SmartGraphAttribute { get; }
}
