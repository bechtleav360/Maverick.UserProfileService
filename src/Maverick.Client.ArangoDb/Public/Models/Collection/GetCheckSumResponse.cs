using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Response class of type SingleApiResponse<see cref="SingleApiResponse{T}" /> containing an object with collection
///     properties, the revision id and the calculated checksum  <see cref="CollectionWithCheckSumAndRevisionIdEntity" />
/// </summary>
/// <inheritdoc />
public class GetCheckSumResponse : SingleApiResponse<CollectionWithCheckSumAndRevisionIdEntity>
{
    internal GetCheckSumResponse(Response response, CollectionWithCheckSumAndRevisionIdEntity collection) : base(
        response,
        collection)
    {
    }

    internal GetCheckSumResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
