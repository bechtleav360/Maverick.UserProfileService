namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Contains information about the number of indexes defined for a collection and the total memory allocated for
///     indexes in bytes
/// </summary>
public class Indexes
{
    /// <summary>
    ///     The number of indexes defined for the collection, including the pre-defined indexes (e.g primary index).
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     The total memory allocated for indexes in bytes.
    /// </summary>
    public int Size { get; set; }
}
