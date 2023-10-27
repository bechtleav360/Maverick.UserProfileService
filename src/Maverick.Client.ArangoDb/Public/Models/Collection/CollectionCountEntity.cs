namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Class containing detailed collection information (by calling a method to get the number of documents in this
///     collection)
/// </summary>
/// <inheritdoc />
public class CollectionCountEntity : CollectionWithDetailEntity
{
    /// <summary>
    ///     The number of documents currently present in the collection
    /// </summary>
    public int? Count { get; set; }
}
