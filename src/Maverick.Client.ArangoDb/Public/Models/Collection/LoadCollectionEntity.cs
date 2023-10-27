namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Class containing collection information get by loading
/// </summary>
public class LoadCollectionEntity : CollectionEntity
{
    /// <summary>
    ///     The number of documents currently present in the collection
    /// </summary>
    public int? Count { get; set; }
}
