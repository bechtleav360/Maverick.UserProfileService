namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains options that canbe set by starting a transaction.
/// </summary>
public class TransactionOptions
{
    /// <summary>
    ///     The maximum time to wait for required locks to be released, before the transaction times out.
    /// </summary>
    public long? LockTimeout { get; set; }

    /// <summary>
    ///     The maximum transaction size before making intermediate commits (RocksDB only).
    /// </summary>
    public long? MaxTransactionSize { get; set; }

    /// <summary>
    ///     an optional boolean flag that, if set, will force the
    ///     transaction to write all data to disk before returning.
    /// </summary>
    public bool? WaitForSync { get; set; }
}
