using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains a list of edges with some edges statistics
/// </summary>
public class EdgesResponseEntity
{
    /// <summary>
    ///     a list of edges starting or ending in the vertex identified by vertex-handle
    /// </summary>
    public List<Edge> Edges { get; set; }

    /// <summary>
    ///     some edge statistics
    /// </summary>
    public EdgeStats Stats { get; set; }
}
