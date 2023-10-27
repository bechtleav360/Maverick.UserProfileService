namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some edges statistics
/// </summary>
public class EdgeStats
{
    /// <summary>
    ///     filtered edges
    /// </summary>
    public int Filtered { get; set; }

    /// <summary>
    ///     The number of scanned indexes
    /// </summary>
    public int ScannedIndex { get; set; }
}
