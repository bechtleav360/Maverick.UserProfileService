using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a boolean value which takes
///     the value true when the journal of the given collection has been successful rotated
/// </summary>
/// <inheritdoc />
public class RotateJournalResponse : SingleApiResponse<bool>
{
    internal RotateJournalResponse(Response response, bool result) : base(response, result)
    {
    }

    internal RotateJournalResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
