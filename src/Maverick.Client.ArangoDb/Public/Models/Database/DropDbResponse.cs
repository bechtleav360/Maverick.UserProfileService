using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a boolean value which takes
///     the value true only when the database has been succesful dropped
/// </summary>
/// <inheritdoc />
public class DropDbResponse : SingleApiResponse<bool>
{
    internal DropDbResponse(Response response, bool result) : base(response, result)
    {
    }

    internal DropDbResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
