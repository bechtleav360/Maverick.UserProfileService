using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Protocol;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Response class of type SingleApiResponse <see cref="SingleApiResponse{T}" /> containing a list of all running
///     transactions <see cref="TransactionEntity" />
/// </summary>
/// <inheritdoc />
public class GetAllRunningTransactionsResponse : SingleApiResponse<IList<Transaction>>
{
    internal GetAllRunningTransactionsResponse(Response response, IList<Transaction> transactions) : base(
        response,
        transactions)
    {
    }

    internal GetAllRunningTransactionsResponse(Response response, Exception exception) : base(response, exception)
    {
    }
}
