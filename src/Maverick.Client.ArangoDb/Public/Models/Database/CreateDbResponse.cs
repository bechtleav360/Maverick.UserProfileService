using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a boolean value which takes
///     the value true only when the database has been successful created
/// </summary>
/// <inheritdoc />
public class CreateDbResponse : SingleApiResponse<bool>
{
    internal CreateDbResponse(Response response, bool result) : base(response, result)
    {
    }

    internal CreateDbResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
