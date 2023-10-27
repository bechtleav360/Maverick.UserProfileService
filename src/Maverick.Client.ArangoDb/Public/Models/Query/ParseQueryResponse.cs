using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with the
///     attributes of the parsed query  <see cref="ParseQueryResponseEntity" />
/// </summary>
/// <inheritdoc />
public class ParseQueryResponse : SingleApiResponse<ParseQueryResponseEntity>
{
    internal ParseQueryResponse(Response response, ParseQueryResponseEntity queryInfos) : base(response, queryInfos)
    {
    }

    internal ParseQueryResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
