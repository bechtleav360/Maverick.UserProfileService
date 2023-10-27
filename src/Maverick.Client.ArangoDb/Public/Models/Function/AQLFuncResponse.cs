using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Function;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a boolean value which takes
///     the value true if the (un)registration of the AQL function has been successful
/// </summary>
/// <inheritdoc />
public class AqlFuncResponse : SingleApiResponse<bool>
{
    internal AqlFuncResponse(Response response, bool result) : base(response, result)
    {
    }

    internal AqlFuncResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
