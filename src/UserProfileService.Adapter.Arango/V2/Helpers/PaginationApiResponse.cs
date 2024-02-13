using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Represents a pagination API response.
/// </summary>
public class PaginationApiResponse : IApiResponse
{
    /// <summary>
    ///     Gets the execution time of the response in milliseconds.
    /// </summary>
    public long ExecutionTime { get; }

    /// <summary>
    ///     Gets the original counting response containing the total item counts.
    /// </summary>
    public MultiApiResponse<CountingModel> OriginalCountingResponse { get; }

    /// <summary>
    ///     Gets the original selection response.
    /// </summary>
    public MultiApiResponse OriginalSelectionResponse { get; }

    /// <summary>
    ///     Gets the total amount from the counting response.
    /// </summary>
    public long TotalAmount { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationApiResponse"/> class.
    /// </summary>
    /// <param name="selectionResponse">The original selection response.</param>
    /// <param name="countingResponse">The original counting response.</param>
    public PaginationApiResponse(
        MultiApiResponse selectionResponse,
        MultiApiResponse<CountingModel> countingResponse)
    {
        ExecutionTime = selectionResponse?.ExecutionTime ?? -1;
        OriginalSelectionResponse = selectionResponse;
        TotalAmount = countingResponse?.QueryResult?.FirstOrDefault()?.DocumentCount ?? -1;
        OriginalCountingResponse = countingResponse;
    }
}

/// <summary>
///     Represents a pagination API response with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result elements.</typeparam>
public class PaginationApiResponse<TResult> : PaginationApiResponse
{
    /// <summary>
    ///     Gets the original selection response with the specified result type.
    /// </summary>
    public new MultiApiResponse<TResult> OriginalSelectionResponse { get; }

    /// <summary>
    ///     Gets the query result as a read-only list of elements.
    /// </summary>
    public IReadOnlyList<TResult> QueryResult { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationApiResponse{TResult}"/> class.
    /// </summary>
    /// <param name="elements">The elements to populate the query result.</param>
    internal PaginationApiResponse(IEnumerable<TResult> elements) : base(null, null)
    {
        QueryResult = elements?.ToList() ?? new List<TResult>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginationApiResponse{TResult}"/> class.
    /// </summary>
    /// <param name="selectionResponse">The original selection response with the specified result type.</param>
    /// <param name="countingResponse">The original counting response.</param>
    public PaginationApiResponse(
        MultiApiResponse<TResult> selectionResponse,
        MultiApiResponse<CountingModel> countingResponse) : base(selectionResponse, countingResponse)
    {
        QueryResult = selectionResponse.QueryResult;
        OriginalSelectionResponse = selectionResponse;
    }
}
