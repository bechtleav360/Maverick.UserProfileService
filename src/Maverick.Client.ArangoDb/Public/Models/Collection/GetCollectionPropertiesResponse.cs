using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with detailed
///     informations about a collection <see cref="CollectionWithDetailEntity" />
/// </summary>
/// <inheritdoc />
public class GetCollectionPropertiesResponse : SingleApiResponse<CollectionWithDetailEntity>
{
    internal GetCollectionPropertiesResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    internal GetCollectionPropertiesResponse(Response response, CollectionWithDetailEntity collection) : base(
        response,
        collection)
    {
    }
}
