using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Administration;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing ServerInfos
///     <see cref="ServerInfos" />
/// </summary>
public class GetServerVersionResponse : SingleApiResponse<ServerInfos>
{
    internal GetServerVersionResponse(Response response, ServerInfos infos) : base(response, infos)
    {
    }

    internal GetServerVersionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
