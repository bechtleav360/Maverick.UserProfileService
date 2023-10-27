using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     informations about the unloaded collection <see cref="CollectionEntity" />
/// </summary>
/// <inheritdoc />
public class UnloadCollectionResponse : SingleApiResponse<CollectionEntity>
{
    internal UnloadCollectionResponse(Response response, CollectionEntity collection) : base(response, collection)
    {
    }

    internal UnloadCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
