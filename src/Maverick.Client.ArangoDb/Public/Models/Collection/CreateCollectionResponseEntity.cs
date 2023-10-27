namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Object containing attributes that has been returned by creating a collection
/// </summary>
/// <inheritdoc />
public class CreateCollectionResponseEntity : CollectionEntity
{
    /// <summary>
    ///     is the cache enabled return true else false
    /// </summary>
    public string CacheEnabled { get; set; }

    /// <summary>
    ///     Collection key options
    /// </summary>
    public KeyOptions KeyOptions { get; set; }

    /// <summary>
    ///     Object id derived from collection Id
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    ///     any of: ["unloaded", "loading", "loaded", "unloading", "deleted", "unknown"] Only relevant for the MMFiles storage
    ///     engine
    /// </summary>
    public string StatusString { get; set; }

    /// <summary>
    ///     If true then creating, changing or removing
    ///     documents will wait until the data has been synchronized to disk.
    /// </summary>
    public bool WaitForSync { get; set; }
}
