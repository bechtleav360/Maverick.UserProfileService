using System;

namespace Maverick.Client.ArangoDb.Public.Exceptions;

/// <summary>
///     The exception that is thrown when a transaction is not running. That can happen, if it is not existent, or already
///     committed or aborted.
/// </summary>
public class TransactionNotRunningException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="TransactionNotRunningException" /> with a specified transaction id.
    /// </summary>
    public TransactionNotRunningException(string transactionId) : base(
        $"Transaction with id '{transactionId}' is not valid (either not running, already committed or aborted or not existent).")
    {
    }
}
