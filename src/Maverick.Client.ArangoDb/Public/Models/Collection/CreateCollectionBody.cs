namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contain properties that can be used to create a collection
/// </summary>
/// <inheritdoc />
public class CreateCollectionBody : CreateCollectionOptions
{
    /// <summary>
    ///     The name of the collection
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Type of the collection
    /// </summary>
    public ACollectionType Type { get; set; }
}
