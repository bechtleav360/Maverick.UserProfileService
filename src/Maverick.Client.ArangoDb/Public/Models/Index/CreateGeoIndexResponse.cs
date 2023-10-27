using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing information about the
///     created geo index <see cref="GeoIndexResponseEntity" />
/// </summary>
/// <inheritdoc />
public class CreateGeoIndexResponse : SingleApiResponse<GeoIndexResponseEntity>
{
    internal CreateGeoIndexResponse(Response response, GeoIndexResponseEntity index) : base(response, index)
    {
    }

    internal CreateGeoIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
