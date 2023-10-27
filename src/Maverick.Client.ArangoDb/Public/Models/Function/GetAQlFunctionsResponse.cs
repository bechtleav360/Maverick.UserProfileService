using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Function;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of registered AQL
///     user functions <see cref="FunctionEntity" />
/// </summary>
/// <inheritdoc />
public class GetAQlFunctionsResponse : SingleApiResponse<IList<FunctionEntity>>
{
    internal GetAQlFunctionsResponse(Response response, IList<FunctionEntity> functions) : base(response, functions)
    {
    }

    internal GetAQlFunctionsResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
