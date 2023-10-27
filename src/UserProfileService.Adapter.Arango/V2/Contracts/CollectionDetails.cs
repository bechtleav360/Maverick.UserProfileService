using Maverick.Client.ArangoDb.Public;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     The Collection details that contain the type and name.
/// </summary>
public class CollectionDetails
{
    /// <summary>
    ///     The name of the collection.
    /// </summary>
    public string CollectionName { set; get; }

    /// <summary>
    ///     The type of the collection (<see cref="ACollectionType.Edge" /> or <see cref="ACollectionType.Document" />).
    /// </summary>
    public ACollectionType CollectionType { set; get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return CollectionName;
    }
}
