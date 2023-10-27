using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contain attributes that can be used by creating a new collection
/// </summary>
public class CreateCollectionOptions
{
    /// <summary>
    ///     (The default is ""): in an Enterprise Edition cluster, this attribute binds
    ///     the specifics of sharding for the newly created collection to follow that of a
    ///     specified existing collection
    /// </summary>
    public string DistributeShardsLike { get; set; }

    /// <summary>
    ///     whether or not the collection will be compacted (default is true)
    ///     This option is meaningful for the MMFiles storage engine only.
    /// </summary>
    public bool? DoCompact { get; set; }

    /// <summary>
    ///     The number of buckets into which indexes using a hash
    ///     table are split.The default is 16 and this number has to be a
    ///     power of 2 and less than or equal to 1024
    /// </summary>
    public int? IndexBuckets { get; set; }

    /// <summary>
    ///     If true, create a system collection. In this case collection-name
    ///     should start with an underscore.End users should normally create non-system
    ///     collections only
    /// </summary>
    public bool? IsSystem { get; set; }

    /// <summary>
    ///     If true then the collection data is kept in-memory only and not made persistent.
    /// </summary>
    public bool? IsVolatile { get; set; }

    /// <summary>
    ///     The maximal size of a journal or datafile in bytes
    /// </summary>
    public long? JournalSize { get; set; }

    public CollectionKeyOptions KeyOptions { get; set; }

    /// <summary>
    ///     (The default is 1): in a cluster, this value determines the
    ///     number of shards to create for the collection.In a single
    ///     server setup, this option is meaningless
    /// </summary>
    public int? NumberOfShards { get; set; }

    /// <summary>
    ///     The default is 1): in a cluster, this attribute determines how many copies
    ///     of each shard are kept on different DBServers.The value 1 means that only one
    ///     copy (no synchronous replication) is kept.A value of k means that k-1 replicas
    ///     are kept.Any two copies reside on different DBServers
    /// </summary>
    public int? ReplicationFactor { get; set; }

    /// <summary>
    ///     Optional object that specifies the collection level schema for documents (only available in ArangoDB ver. 3.7.1 and
    ///     upwards).<br />
    ///     Its properties rule, level and message must follow the rules documented in
    ///     <see href="https://www.arangodb.com/docs/3.7/document-schema-validation.html" />.
    /// </summary>
    public ACollectionSchema Schema { get; set; }

    /// <summary>
    ///     ArangoDB sharding strategy
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public AShardingStrategy ShardingStrategy { get; set; }

    /// <summary>
    ///     (The default is [ "_key" ]): in a cluster, this attribute determines
    ///     which document attributes are used to determine the target shard for documents
    /// </summary>
    public string ShardKeys { get; set; }

    /// <summary>
    ///     In an Enterprise Edition cluster, this attribute determines an attribute
    ///     of the collection that must contain the shard key value of the referred-to
    ///     smart join collection
    /// </summary>
    public string SmartJoinAttribute { get; set; }

    /// <summary>
    ///     If true then the data is synchronized to disk before returning from a
    ///     document create, update, replace or removal operation. (default: false)
    /// </summary>
    public bool? WaitForSync { get; set; }

    /// <summary>
    ///     Write concern for this collection (default: 1). It determines how many copies of each shard are required to be in
    ///     sync on the different DBServers
    /// </summary>
    public int? WriteConcern { get; set; }
}
