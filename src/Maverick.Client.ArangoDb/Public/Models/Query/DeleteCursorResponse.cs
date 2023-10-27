using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a boolean with takes the
///     value true when the cursor has been successful deleted
/// </summary>
/// <inheritdoc />
public class DeleteCursorResponse : SingleApiResponse<bool>
{
    internal DeleteCursorResponse(Response response, bool status) : base(response, status)
    {
    }

    internal DeleteCursorResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
