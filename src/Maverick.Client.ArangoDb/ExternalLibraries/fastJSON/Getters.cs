using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

/// <summary>
///     Represents the structure or schema of a data source.
/// </summary>
public sealed class DataSetSchema
{
    /// <summary>
    ///     Additional information related to the schema.
    /// </summary>
    public List<string> Info;

    /// <summary>
    ///     The name associated with the schema.
    /// </summary>
    public string Name;
}
