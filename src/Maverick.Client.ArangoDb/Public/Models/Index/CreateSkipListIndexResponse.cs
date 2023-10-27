using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing information about the
///     created (skiplist) index <see cref="IndexResponseWithSelectivityEntity" />
/// </summary>
/// <inheritdoc />
public class CreateSkipListIndexResponse : SingleApiResponse<IndexResponseWithSelectivityEntity>
{
    internal CreateSkipListIndexResponse(Response response, IndexResponseWithSelectivityEntity index) : base(
        response,
        index)
    {
    }

    internal CreateSkipListIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
