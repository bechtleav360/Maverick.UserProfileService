namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contains some information about a collection
/// </summary>
/// <inheritdoc />
public class CollectionWithDetailEntity : CollectionEntity
{
    /// <summary>
    ///     is Cache enable ? -> true else false
    /// </summary>
    public bool? CacheEnabled { get; set; }

    /// <summary>
    ///     the number of index buckets
    ///     Only relevant for the MMFiles storage engine
    /// </summary>
    public int? IndexBuckets { get; set; }

    /// <summary>
    ///     if true then the collection data will be
    ///     kept in memory only and ArangoDB will not write or sync the data
    ///     to disk.This option is only present for the MMFiles storage engine.
    /// </summary>
    public bool? IsVolatile { get; set; }

    /// <summary>
    ///     he maximal size setting for journals / datafiles
    ///     in bytes.This option is only present for the MMFiles storage engine.
    /// </summary>
    public int? JournalSize { get; set; }

    /// <summary>
    ///     Key options of the collection
    /// </summary>
    public KeyOptions KeyOptions { get; set; }

    /// <summary>
    ///     contains how many minimal copies of each shard need to be in sync on different DBServers.
    /// </summary>
    public int? MinReplicationFactor { get; set; }

    /// <summary>
    ///     Object ID derived from the collection Id
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    ///     Contains how many copies of each shard are kept on different DBServers
    /// </summary>
    public int? ReplicationFactor { get; set; }

    /// <summary>
    ///     the sharding strategy selected for the collection
    /// </summary>
    public string ShardingStrategy { get; set; }

    /// <summary>
    ///     contains the names of document attributes that are used to
    ///     determine the target shard for documents
    /// </summary>
    public string ShardKeys { get; set; }

    /// <summary>
    ///     Attribute that is used in smart graphs,
    /// </summary>
    public string SmartGraphAttribute { get; set; }

    /// <summary>
    ///     any of: ["unloaded", "loading", "loaded", "unloading", "deleted", "unknown"] Only relevant for the MMFiles storage
    ///     engine
    /// </summary>
    public string StatusString { get; set; }

    /// <summary>
    ///     If true then creating, changing or removing
    ///     documents will wait until the data has been synchronized to disk.
    /// </summary>
    public bool? WaitForSync { get; set; }
}
