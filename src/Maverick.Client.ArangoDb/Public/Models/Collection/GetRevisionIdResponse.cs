using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the revision id of a given
///     collection
/// </summary>
/// <inheritdoc />
public class GetRevisionIdResponse : SingleApiResponse<string>
{
    internal GetRevisionIdResponse(Response response, string revisionId) : base(response, revisionId)
    {
    }

    internal GetRevisionIdResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
