using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> contains information about the new
///     created index <see cref="FullTextIndexResponseEntity" />
/// </summary>
/// <inheritdoc />
public class CreateFullTextIndexResponse : SingleApiResponse<FullTextIndexResponseEntity>
{
    internal CreateFullTextIndexResponse(Response response, FullTextIndexResponseEntity index) : base(
        response,
        index)
    {
    }

    internal CreateFullTextIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
