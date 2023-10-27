namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Object containing some collection properties that can be modified after the creation of the collection.
/// </summary>
public class CollectionPropertyEntity
{
    /// <summary>
    ///     The maximal size of a journal or datafile in bytes
    /// </summary>
    public long? JournalSize { get; set; }

    /// <summary>
    ///     Name of the collection
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     If true then creating or changing a document will wait until the data has been synchronized to disk.
    /// </summary>
    public bool? WaitForSync { get; set; }
}
