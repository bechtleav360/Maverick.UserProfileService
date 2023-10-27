using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing the transaction status as
///     string
/// </summary>
/// <inheritdoc />
public class GetTransactionStatusResponse : SingleApiResponse<TransactionStatus>
{
    internal GetTransactionStatusResponse(Response response, TransactionStatus status) : base(response, status)
    {
    }

    internal GetTransactionStatusResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
