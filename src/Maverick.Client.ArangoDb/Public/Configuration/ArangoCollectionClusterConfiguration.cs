using System;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Contains configuration properties of newly generated collections in a cluster.
/// </summary>
public class ArangoCollectionClusterConfiguration
{
    /// <summary>
    ///     Gets or sets the number of shards for the collection.
    /// </summary>
    public int? NumberOfShards { get; set; }

    /// <summary>
    ///     Gets or sets the replication factor for the collection.
    /// </summary>
    public int? ReplicationFactor { get; set; }

    /// <summary>
    ///     Gets or sets the write concern for the collection.
    /// </summary>
    public int? WriteConcern { get; set; }

    /// <summary>
    ///     Determines whether this <see cref="ArangoCollectionClusterConfiguration"/> instance is equal to another.
    /// </summary>
    /// <param name="other">The other <see cref="ArangoCollectionClusterConfiguration"/> instance to compare.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    protected bool Equals(ArangoCollectionClusterConfiguration other)
    {
        return NumberOfShards == other.NumberOfShards
            && ReplicationFactor == other.ReplicationFactor
            && WriteConcern == other.WriteConcern;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ArangoCollectionClusterConfiguration)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(NumberOfShards, ReplicationFactor, WriteConcern);
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
