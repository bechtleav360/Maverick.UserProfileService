using System;
using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Contains all configuration settings for collections generated in a cluster.
/// </summary>
public class ArangoClusterConfiguration
{
    private Dictionary<string, ArangoCollectionClusterConfiguration> _documentCollections;
    private Dictionary<string, ArangoCollectionClusterConfiguration> _edgeCollections;

    /// <summary>
    ///     Cluster configuration per pattern of document collection names (i.e. * as a key means all document collections).
    /// </summary>
    public Dictionary<string, ArangoCollectionClusterConfiguration> DocumentCollections
    {
        get => _documentCollections;
        set =>
            _documentCollections = value != null
                ? new Dictionary<string, ArangoCollectionClusterConfiguration>(
                    value,
                    StringComparer.OrdinalIgnoreCase)
                : null;
    }

    /// <summary>
    ///     Cluster configuration per pattern of edge collection names (i.e. * as a key means all edge collections).
    /// </summary>
    public Dictionary<string, ArangoCollectionClusterConfiguration> EdgeCollections
    {
        get => _edgeCollections;
        set =>
            _edgeCollections = value != null
                ? new Dictionary<string, ArangoCollectionClusterConfiguration>(
                    value,
                    StringComparer.OrdinalIgnoreCase)
                : null;
    }

    /// <summary>
    ///     Determines whether the specified object is equal do the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>
    ///     <c>true</c> if both objects are equal, otherwise <c>false</c>. If both objects are null, the method returns
    ///     <c>true</c>.
    /// </returns>
    protected bool Equals(ArangoClusterConfiguration other)
    {
        return Equals(DocumentCollections, other.DocumentCollections)
            && Equals(EdgeCollections, other.EdgeCollections);
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

        return Equals((ArangoClusterConfiguration)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(DocumentCollections, EdgeCollections);
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}
