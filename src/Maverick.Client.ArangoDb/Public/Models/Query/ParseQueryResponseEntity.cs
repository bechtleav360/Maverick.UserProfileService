using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains informations about the parsed query.
/// </summary>
public class ParseQueryResponseEntity
{
    /// <summary>
    ///     internal representation of the query
    /// </summary>
    public IEnumerable<Dictionary<string, object>> Ast { get; set; }

    /// <summary>
    ///     bind parameters
    /// </summary>
    public IEnumerable<Dictionary<string, object>> BindVars { get; set; }

    /// <summary>
    ///     collection names used in the query
    /// </summary>
    public IList<string> Collections { get; set; }

    /// <summary>
    ///     boolean value taking the value true when the query is syntactilly valid
    /// </summary>
    public bool Parsed { get; set; }
}
