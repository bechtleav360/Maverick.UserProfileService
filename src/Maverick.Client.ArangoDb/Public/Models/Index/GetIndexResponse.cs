using System;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Models.Collection;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing an index object
///     <see cref="CollectionIndex" />
/// </summary>
public class GetIndexResponse : SingleApiResponse<CollectionIndex>
{
    internal GetIndexResponse(Response response, CollectionIndex index) : base(response, index)
    {
    }

    internal GetIndexResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
