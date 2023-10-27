using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing information about the
///     created (persistent) index <see cref="IndexResponseWithSelectivityEntity" />
/// </summary>
/// <inheritdoc />
public class CreatePersistentIndexResponse : SingleApiResponse<IndexResponseWithSelectivityEntity>
{
    internal CreatePersistentIndexResponse(Response response, IndexResponseWithSelectivityEntity index) : base(
        response,
        index)
    {
    }

    internal CreatePersistentIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
