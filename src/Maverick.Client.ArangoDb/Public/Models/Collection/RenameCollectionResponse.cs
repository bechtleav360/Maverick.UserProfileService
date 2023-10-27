using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     informations about the renamed collection <see cref="CollectionEntity" />
/// </summary>
/// <inheritdoc />
public class RenameCollectionResponse : SingleApiResponse<CollectionEntity>
{
    internal RenameCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    internal RenameCollectionResponse(Response response, CollectionEntity collection) : base(response, collection)
    {
    }
}
