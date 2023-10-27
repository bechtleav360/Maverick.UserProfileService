using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing some information about the
///     current database <see cref="DatabaseInfoEntity" />
/// </summary>
/// <inheritdoc />
public class GetCurrentDatabaseResponse : SingleApiResponse<DatabaseInfoEntity>
{
    internal GetCurrentDatabaseResponse(Response response, DatabaseInfoEntity entity) : base(response, entity)
    {
    }

    internal GetCurrentDatabaseResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
