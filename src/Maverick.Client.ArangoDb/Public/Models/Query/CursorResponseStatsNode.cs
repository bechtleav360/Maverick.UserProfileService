namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains stats related to the nodes
/// </summary>
public class CursorResponseStatsNode
{
    /// <summary>
    ///     Number of calls
    /// </summary>
    public long Calls { get; set; }

    /// <summary>
    ///     Node Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Number of items.
    /// </summary>
    public long Items { get; set; }

    /// <summary>
    ///     Nodes runtime
    /// </summary>
    public double Runtime { get; set; }
}
