using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     API Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a collection entity
///     with some details properties <see cref="CollectionWithDetailEntity" />
/// </summary>
public class ChangePropertiesResponse : SingleApiResponse<CollectionWithDetailEntity>
{
    internal ChangePropertiesResponse(Response response, CollectionWithDetailEntity collection) : base(
        response,
        collection)
    {
    }

    internal ChangePropertiesResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
