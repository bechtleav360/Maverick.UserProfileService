using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with detailed
///     collection informations <see cref="CollectionCountEntity" />
/// </summary>
/// <inheritdoc />
public class GetCollectionCountResponse : SingleApiResponse<CollectionCountEntity>
{
    internal GetCollectionCountResponse(Response response, CollectionCountEntity collection) : base(
        response,
        collection)
    {
    }

    internal GetCollectionCountResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
