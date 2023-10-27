using Maverick.Client.ArangoDb.Public.Models.Query;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Marks a part of a response that contain detailed information of the cursor request.
/// </summary>
public interface ICursorInnerResponse
{
    /// <summary>
    ///     Indicates whether the query result was served from the query cache or not.
    ///     If the query result is served from the query cache, the extra return attribute
    ///     will not contain any stats sub-attribute and no profile sub-attribute.
    /// </summary>
    public bool Cached { get; set; }

    /// <summary>
    ///     the total number of result documents available
    ///     (only available if the query was executed with the count attribute set)
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
    ///     Whether there are more results available for the cursor on the server.
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    ///     ID of temporary cursor created on the server (optional).
    /// </summary>
    public string Id { get; set; }
}

/// <summary>
///     Marks a part of a response that contain detailed information of the cursor request. It also marks the inner type of
///     the implementation without using it.
/// </summary>
// ReSharper disable once UnusedTypeParameter
public interface ICursorInnerResponse<TContainer> : ICursorInnerResponse
{
}
