using System;
using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Contains options that can be set by executing a JavaScript transaction.
/// </summary>
/// <inheritdoc />
public class JsTransactionOptions : TransactionOptions
{
    /// <summary>
    ///     Parameters to be passed into the transaction JavaScript function defined by <see cref="Action" />.
    /// </summary>
    public Dictionary<string, object> Params { get; set; }
}
