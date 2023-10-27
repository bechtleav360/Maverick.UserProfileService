using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

internal class PaginationApiResponse : IApiResponse
{
    public long ExecutionTime { get; }

    public MultiApiResponse<CountingModel> OriginalCountingResponse { get; }

    public MultiApiResponse OriginalSelectionResponse { get; }

    public long TotalAmount { get; }

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

internal class PaginationApiResponse<TResult> : PaginationApiResponse
{
    public new MultiApiResponse<TResult> OriginalSelectionResponse { get; }
    public IReadOnlyList<TResult> QueryResult { get; }

    internal PaginationApiResponse(IEnumerable<TResult> elements) : base(null, null)
    {
        QueryResult = elements?.ToList() ?? new List<TResult>();
    }

    public PaginationApiResponse(
        MultiApiResponse<TResult> selectionResponse,
        MultiApiResponse<CountingModel> countingResponse) : base(selectionResponse, countingResponse)
    {
        QueryResult = selectionResponse.QueryResult;
        OriginalSelectionResponse = selectionResponse;
    }
}
