using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with the list of
///     edges and some edges statistics  <see cref="EdgesResponseEntity" />
/// </summary>
/// <inheritdoc />
public class GetEdgesResponse : SingleApiResponse<EdgesResponseEntity>
{
    internal GetEdgesResponse(Response response, EdgesResponseEntity edges) : base(response, edges)
    {
    }

    internal GetEdgesResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
