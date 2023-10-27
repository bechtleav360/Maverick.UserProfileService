using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     object with an attribute indexes containing an array of all
///     index descriptions for the given collection.The same information is also
///     available in the identifiers as an object with the index handles as
///     keys.
/// </summary>
public class GetAllIndexesEntity
{
    /// <summary>
    ///     dictionary of all index descriptions for a given collection with the index handles as keys
    /// </summary>
    public Dictionary<string, CollectionIndex> Identifiers { get; set; }

    /// <summary>
    ///     array of all index descriptions for a given collection
    /// </summary>
    public CollectionIndex[] Indexes { get; set; }
}
