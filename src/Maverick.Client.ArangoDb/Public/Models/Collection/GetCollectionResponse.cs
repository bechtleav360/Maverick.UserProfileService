using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     informations about a collection <see cref="CollectionEntity" />
/// </summary>
/// <inheritdoc />
public class GetCollectionResponse : SingleApiResponse<CollectionEntity>
{
    internal GetCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    internal GetCollectionResponse(Response response, CollectionEntity collection) : base(response, collection)
    {
    }
}
