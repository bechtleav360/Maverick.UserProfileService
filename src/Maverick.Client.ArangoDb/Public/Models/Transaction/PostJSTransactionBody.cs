using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Transaction;

/// <summary>
///     Represents information required to make a javascript transaction request to ArangoDB.
/// </summary>
/// <inheritdoc />
public class PostJsTransactionBody : PostTransactionBody
{
    /// <summary>
    ///     JavaScript function describing the transaction action.
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    ///     Parameters to be passed into the transaction JavaScript function defined by <see cref="Action" />.
    /// </summary>
    public Dictionary<string, object> Params { get; set; }
}
