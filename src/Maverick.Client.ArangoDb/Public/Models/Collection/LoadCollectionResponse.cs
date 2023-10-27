using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with some
///     information about the loaded collection <see cref="LoadCollectionEntity" />
/// </summary>
/// <inheritdoc />
public class LoadCollectionResponse : SingleApiResponse<LoadCollectionEntity>
{
    internal LoadCollectionResponse(Response response, LoadCollectionEntity collection) : base(response, collection)
    {
    }

    internal LoadCollectionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
