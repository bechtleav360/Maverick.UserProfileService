namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contains some collections statistics
/// </summary>
public class Figures
{
    /// <summary>
    ///     is true wheen the cache is using else false
    /// </summary>
    public bool CacheInUse { get; set; }

    /// <summary>
    ///     The size of the cache
    /// </summary>
    public int CacheSize { get; set; }

    /// <summary>
    ///     used cache memory
    /// </summary>
    public int CacheUsage { get; set; }

    /// <summary>
    ///     The size of the documents (in the specified collection)
    /// </summary>
    public int DocumentsSize { get; set; }

    /// <summary>
    ///     collection indexes
    /// </summary>
    public Indexes Indexes { get; set; }
}
