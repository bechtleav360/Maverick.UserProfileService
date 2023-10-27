using System;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Contains configuration properties of newly generated collections in a cluster.
/// </summary>
public class ArangoCollectionClusterConfiguration
{
    public int? NumberOfShards { get; set; }
    public int? ReplicationFactor { get; set; }
    public int? WriteConcern { get; set; }

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
