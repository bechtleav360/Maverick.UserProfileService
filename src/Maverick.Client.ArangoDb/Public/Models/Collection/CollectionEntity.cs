namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Content the basic properties of a collection.
/// </summary>
public class CollectionEntity
{
    /// <summary>
    ///     Unique identifier of the collection
    /// </summary>
    public string GloballyUniqueId { get; set; }

    /// <summary>
    ///     Collection Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     true if this is a system collection; usually name will start with an underscore.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    ///     The name of the collection
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     corresponds to statusString; Only relevant for the MMFiles storage engine
    /// </summary>
    public ACollectionStatus Status { get; set; }

    /// <summary>
    ///     The type of the collection: 0: "collection", 2: "regular document collection", 3: "edge collection"
    /// </summary>
    public ACollectionType Type { get; set; }
}
