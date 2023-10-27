using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     collection informations <see cref="CollectionWithRevisionEntity" />
/// </summary>
/// <inheritdoc />
public class GetRevisionResponse : SingleApiResponse<CollectionWithRevisionEntity>
{
    internal GetRevisionResponse(Response response, CollectionWithRevisionEntity collection) : base(
        response,
        collection)
    {
    }

    internal GetRevisionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
