using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing information about the
///     created hash index <see cref="IndexResponseWithSelectivityEntity" />
/// </summary>
/// <inheritdoc />
public class CreateHashIndexResponse : SingleApiResponse<IndexResponseWithSelectivityEntity>
{
    internal CreateHashIndexResponse(Response response, IndexResponseWithSelectivityEntity index) : base(
        response,
        index)
    {
    }

    internal CreateHashIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
