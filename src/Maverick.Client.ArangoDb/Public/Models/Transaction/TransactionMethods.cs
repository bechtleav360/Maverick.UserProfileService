using System;
using Maverick.Client.ArangoDb.Public.Exceptions;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Represents a base class for all transaction method classes.
/// </summary>
public abstract class TransactionMethods
{
    /// <summary>
    ///     Gets the running transaction.
    /// </summary>
    protected IRunningTransaction Transaction { get; }

    internal TransactionMethods(IRunningTransaction transaction)
    {
        Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

        if (transaction.Exception != null)
        {
            throw new ArgumentException(
                "Cannot continue with that transaction. The provided transaction object is in faulted state. See inner exception for details!",
                nameof(transaction),
                transaction.Exception);
        }

        if (transaction.UsedConnection == null)
        {
            throw new ArgumentException(
                "Transaction object must contain a valid connection instance.",
                nameof(transaction));
        }

        if (transaction.GetTransactionStatus() != TransactionStatus.Running)
        {
            throw new TransactionNotRunningException(transaction.GetTransactionId());
        }
    }
}
