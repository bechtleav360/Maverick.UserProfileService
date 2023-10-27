using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains various stats related to the query.
/// </summary>
public class CursorResponseStats
{
    /// <summary>
    ///     Request execution time
    /// </summary>
    public double ExecutionTime { get; set; }

    /// <summary>
    ///     Filtering conditions
    /// </summary>
    public long Filtered { get; set; }

    /// <summary>
    ///     When <see cref="PostCursorOptions.FullCount" /> is used,
    ///     the fullCount attribute will contain the number of documents in the result
    ///     before the last top-level LIMIT in the query was applied.
    /// </summary>
    public long? FullCount { get; set; }

    /// <summary>
    ///     Http Requests
    /// </summary>
    public long HttpRequests { get; set; }

    /// <summary>
    ///     Populated when <see cref="PostCursorOptions.Profile" /> is set to 2.
    /// </summary>
    public IEnumerable<CursorResponseStatsNode> Nodes { get; set; }

    /// <summary>
    ///     mMximum amount of memory the query has used since it was started.
    /// </summary>
    public long PeakMemoryUsage { get; set; }

    /// <summary>
    ///     Full scans
    /// </summary>
    public long ScannedFull { get; set; }

    /// <summary>
    ///     Scanned indexes.
    /// </summary>
    public long ScannedIndex { get; set; }

    /// <summary>
    ///     number executed write operations
    /// </summary>
    public long WritesExecuted { get; set; }

    /// <summary>
    ///     number of ignored writes operations
    /// </summary>
    public long WritesIgnored { get; set; }
}
