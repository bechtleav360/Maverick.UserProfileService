using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of collection
///     entities <see cref="CollectionEntity" />
/// </summary>
/// <inheritdoc />
public class GetAllCollectionsResponse : SingleApiResponse<IList<CollectionEntity>>
{
    internal GetAllCollectionsResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    internal GetAllCollectionsResponse(Response response, IList<CollectionEntity> collections) : base(
        response,
        collections)
    {
    }
}
