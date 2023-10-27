using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the id of the deleted
///     collection.
/// </summary>
public class DeleteCollectionResponse : SingleApiResponse<string>
{
    internal DeleteCollectionResponse(Response response, string collectionId) : base(response, collectionId)
    {
    }

    internal DeleteCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
