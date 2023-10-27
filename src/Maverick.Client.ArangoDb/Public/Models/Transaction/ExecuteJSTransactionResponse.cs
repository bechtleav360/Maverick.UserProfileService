using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the return value (from type
///     {T}) of the transaction.
/// </summary>
/// <typeparam name="T">return type of the transaction function</typeparam>
/// <inheritdoc />
public class ExecuteJsTransactionResponse<T> : SingleApiResponse<T>
{
    internal ExecuteJsTransactionResponse(Response response, T result) : base(response, result)
    {
    }

    internal ExecuteJsTransactionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
