using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing some infos about the
///     started transaction <see cref="TransactionEntity" />
/// </summary>
/// <inheritdoc />
public class BeginTransactionResponse : SingleApiResponse<TransactionEntity>
{
    internal BeginTransactionResponse(Response response, TransactionEntity transaction) : base(
        response,
        transaction)
    {
    }

    internal BeginTransactionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
