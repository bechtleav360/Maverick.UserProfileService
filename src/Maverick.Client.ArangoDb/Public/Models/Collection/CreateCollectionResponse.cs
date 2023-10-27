using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Api Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an collection entity
///     <see cref="CreateCollectionResponseEntity" />
/// </summary>
/// <inheritdoc />
public class CreateCollectionResponse : SingleApiResponse<CreateCollectionResponseEntity>
{
    internal CreateCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }

    internal CreateCollectionResponse(Response response, CreateCollectionResponseEntity collection) : base(
        response,
        collection)
    {
    }
}
