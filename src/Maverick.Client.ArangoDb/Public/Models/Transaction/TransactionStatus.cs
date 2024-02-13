namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains possible value of a transaction status.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    ///     The transaction status is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The transaction is currently running.
    /// </summary>
    Running,

    /// <summary>
    ///     The transaction has been committed.
    /// </summary>
    Committed,

    /// <summary>
    ///     The transaction has been aborted.
    /// </summary>
    Aborted
}
