namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains attributes of a specified transaction
/// </summary>
public class Transaction
{
    /// <summary>
    ///     Transaction ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Status of the transaction
    /// </summary>
    public string State { get; set; }
}
