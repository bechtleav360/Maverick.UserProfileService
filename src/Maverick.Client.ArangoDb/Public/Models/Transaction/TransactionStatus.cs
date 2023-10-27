namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains possible value of a transaction status.
/// </summary>
public enum TransactionStatus
{
    Unknown,
    Running,
    Committed,
    Aborted
}
