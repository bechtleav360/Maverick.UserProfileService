using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Foxx;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with the rsult of
///     the foxx operation.
/// </summary>
public class FoxxResponse<T> : SingleApiResponse<T>
{
    internal FoxxResponse(Response response, T result) : base(response, result)
    {
    }

    internal FoxxResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
