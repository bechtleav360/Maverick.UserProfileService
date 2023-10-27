namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Object contains basic collection properties and the collection revision
/// </summary>
/// <inheritdoc />
public class CollectionWithRevisionEntity : CollectionEntity
{
    /// <summary>
    ///     The collection revision id as a string
    /// </summary>
    public string Revision { get; set; }
}
