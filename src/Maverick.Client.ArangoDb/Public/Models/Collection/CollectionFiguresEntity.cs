namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Class containing statistical information about a collection.
/// </summary>
/// <inheritdoc />
public class CollectionFiguresEntity : CollectionEntity
{
    /// <summary>
    ///     Is true if the cache is enabled (RocksDB storage engine)
    /// </summary>
    public bool CacheEnabled { get; set; }

    /// <summary>
    ///     The number of documents currently present in the collection.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     further statistics about the collection
    /// </summary>
    public Figures Figures { get; set; }

    /// <summary>
    ///     Collection Key options
    /// </summary>
    public KeyOptions KeyOptions { get; set; }

    /// <summary>
    ///     Object Id based of the collection Id
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    ///     collection status as string
    /// </summary>
    public string StatusString { get; set; }

    /// <summary>
    ///     If true then the data is synchronized to disk before returning from a
    ///     document create, update, replace or removal operation. (default: false)
    /// </summary>
    public bool WaitForSync { get; set; }
}
