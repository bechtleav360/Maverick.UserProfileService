using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     A class wrapping an Aql-Query and the parameter to pass.
/// </summary>
public class ParameterizedAql
{
    /// <summary>
    ///     Contains all aql parameter required to execute the query.
    /// </summary>
    public Dictionary<string, object> Parameter { get; set; }

    /// <summary>
    ///     Contains the AQL-Query to execute.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    ///     Contains all collection which the query writes to.
    /// </summary>
    public string[] WriteCollections { get; set; }
}
