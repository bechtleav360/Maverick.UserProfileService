using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Represents the collections object passed in an ArangoDB transaction request.
/// </summary>
public class PostTransactionRequestCollections
{
    /// <summary>
    ///     Collections for which to obtain exclusive locks during a transaction.
    /// </summary>
    public IEnumerable<string> Exclusive { get; set; }

    /// <summary>
    /// The list of read-only collections involved in a transaction.
    /// </summary>
    public IEnumerable<string> Read { get; set; }

    /// <summary>
    ///     The list of write collection involved in a transaction.
    /// </summary>
    public IEnumerable<string> Write { get; set; }
}
