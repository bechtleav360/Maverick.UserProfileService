using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an object with all indexes
///     for the given collection <see cref="GetAllIndexesEntity" />
/// </summary>
/// <inheritdoc />
public class GetAllIndexesResponse : SingleApiResponse<GetAllIndexesEntity>
{
    internal GetAllIndexesResponse(Response response, GetAllIndexesEntity indexes) : base(response, indexes)
    {
    }

    internal GetAllIndexesResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
