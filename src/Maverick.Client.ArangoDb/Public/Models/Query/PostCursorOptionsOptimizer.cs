using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains a list of to-be-included or to-be-excluded optimizer rules
/// </summary>
public class PostCursorOptionsOptimizer
{
    /// <summary>
    ///     A list of to-be-included or to-be-excluded optimizer rules
    /// </summary>
    public IEnumerable<string> Rules { get; set; }
}
