using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Object encapsulating options and parameters of the query.
/// </summary>
public class CreateCursorBody
{
    /// <summary>
    ///     maximum number of result documents to be transferred from the server
    ///     to the client in one roundtrip.If this attribute is not set,
    ///     a server-controlled default value will be used. A batchSize value of 0 is disallowed
    /// </summary>
    public long? BatchSize { get; set; }

    /// <summary>
    ///     Gets or sets a collection of variable names and their bound <see cref="object"/> values.
    /// </summary>
    public Dictionary<string, object> BindVars { get; set; }

    /// <summary>
    ///     flag to determine whether the AQL query results cache
    ///     shall be used.If set to false, then any query cache lookup will be skipped
    ///     for the query.If set to true, it will lead to the query cache being checked
    ///     for the query if the query cache mode is either on or demand.
    /// </summary>
    public bool? Cache { get; set; }

    /// <summary>
    ///     indicates whether the number of documents in the result set should be returned in the “count”
    ///     attribute of the result. Calculating the “count” attribute might have a performance impact for some
    ///     queries in the future so this option is turned off by default, and “count” is only returned when requested
    /// </summary>
    public bool? Count { get; set; }

    /// <summary>
    ///     the maximum number of memory (measured in bytes) that the query is allowed to
    ///     use. If set, then the query will fail with error “resource limit exceeded” in case it allocates too much
    ///     memory. A value of 0 indicates that there is no memory limit.
    /// </summary>
    public long? MemoryLimit { get; set; }

    /// <summary>
    ///     key/value object with extra options for the query.
    /// </summary>
    public PostCursorOptions Options { get; set; }

    /// <summary>
    ///     contains the query string to be executed
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    ///     The time-to-live for the cursor (in seconds). The cursor will be removed on the server
    ///     automatically after the specified amount of time. This is useful to ensure garbage collection of cursors
    ///     that are not fully fetched by clients. If not set, a server-defined value will be used (default: 30 s).
    /// </summary>
    public int? Ttl { get; set; }
}
