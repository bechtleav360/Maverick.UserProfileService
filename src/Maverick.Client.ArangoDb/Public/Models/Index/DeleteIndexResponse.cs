using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the index-handle of the
///     deleted index.
/// </summary>
/// <inheritdoc />
public class DeleteIndexResponse : SingleApiResponse<string>
{
    internal DeleteIndexResponse(Response response, string id) : base(response, id)
    {
    }

    internal DeleteIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
