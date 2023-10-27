using System;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing some infos about the
///     commited transaction <see cref="TransactionEntity" />
/// </summary>
/// <inheritdoc />
public class CommitTransactionResponse : SingleApiResponse<TransactionEntity>
{
    internal CommitTransactionResponse(Response response, TransactionEntity transaction) : base(
        response,
        transaction)
    {
    }

    internal CommitTransactionResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
