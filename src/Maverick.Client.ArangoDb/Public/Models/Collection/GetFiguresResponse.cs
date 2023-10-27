using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with statistical
///     informations about a collection <see cref="CollectionFiguresEntity" />
/// </summary>
/// <inheritdoc />
public class GetFiguresResponse : SingleApiResponse<CollectionFiguresEntity>
{
    internal GetFiguresResponse(Response response, CollectionFiguresEntity collection) : base(response, collection)
    {
    }

    internal GetFiguresResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
