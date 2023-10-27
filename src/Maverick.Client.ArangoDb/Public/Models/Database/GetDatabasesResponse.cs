using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list with the name of
///     accessible databases
/// </summary>
/// <inheritdoc />
public class GetDatabasesResponse : SingleApiResponse<List<string>>
{
    internal GetDatabasesResponse(Response response, List<string> databaseList) : base(response, databaseList)
    {
    }

    internal GetDatabasesResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
