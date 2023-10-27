using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains attributes and documents from the current batch of the cursor
/// </summary>
/// <typeparam name="T">Generic type for the cursor elements</typeparam>
public class PutCursorResponseEntity<T> : ICursorInnerResponse<T>
{
    /// <summary>
    ///     Result documents (might be empty if query has no results).
    /// </summary>
    public bool Cached { get; set; }

    /// <summary>
    ///     if present the total number of elements
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    ///     Optional object with extra information about the query result contained
    ///     in its <see cref="CursorResponseExtra.Stats" /> sub-attribute.
    ///     For data-modification queries, the sub-attribute will contain the number of
    ///     modified documents and the number of documents that could not be modified
    ///     due to an error (if ignoreErrors query option is specified).
    /// </summary>
    public CursorResponseExtra Extra { get; set; }

    /// <summary>
    ///     false if this was the last batch
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    ///     the cursor-identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     a list of documents for the current batch.
    /// </summary>
    public IEnumerable<T> Result { get; set; }
}
