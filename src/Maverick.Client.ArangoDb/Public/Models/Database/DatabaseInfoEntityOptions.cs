using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Additional options set on database level.
/// </summary>
public class DatabaseInfoEntityOptions
{
    /// <summary>
    ///     Default replication factor for new collections created in this database. i.e. a value of 1 will disable replication
    ///     (cluster only).
    /// </summary>
    [JsonProperty(
        PropertyName = "replicationFactor",
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? ReplicationFactor { get; set; }

    /// <summary>
    ///     The sharding method to use for new collections in this database.
    /// </summary>
    [JsonProperty(
        PropertyName = "sharding",
        DefaultValueHandling = DefaultValueHandling.Include)]
    [JsonConverter(typeof(StringEnumConverter))]
    public AShardingMethod Sharding { get; set; }

    /// <summary>
    ///     Default write concern for new collections created in this database.<br />
    ///     It determines how many copies of each shard are required to be in sync on the different DB-Servers.<br />
    ///     If there are less then these many copies in the cluster a shard will refuse to write. Writes to shards with enough
    ///     up-to-date copies will succeed at the same time however. The value of writeConcern can not be larger than
    ///     replicationFactor. (cluster only)
    /// </summary>
    [JsonProperty(
        PropertyName = "writeConcern",
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? WriteConcern { get; set; }
}
