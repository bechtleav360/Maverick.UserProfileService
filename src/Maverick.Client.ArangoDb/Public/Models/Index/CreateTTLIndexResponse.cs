using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing information about the
///     created (TTL) index <see cref="IndexResponseWithSelectivityEntity" />
/// </summary>
/// <inheritdoc />
public class CreateTtlIndexResponse : SingleApiResponse<IndexResponseWithSelectivityEntity>
{
    internal CreateTtlIndexResponse(Response response, IndexResponseWithSelectivityEntity index) : base(
        response,
        index)
    {
    }

    internal CreateTtlIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
