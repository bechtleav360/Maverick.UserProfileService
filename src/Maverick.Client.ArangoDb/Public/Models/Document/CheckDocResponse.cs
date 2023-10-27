using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the revision id if the
///     document has been found
/// </summary>
/// <inheritdoc />
public class CheckDocResponse : SingleApiResponse<string>
{
    internal CheckDocResponse(Response response, string etag) : base(response, etag)
    {
    }

    internal CheckDocResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
