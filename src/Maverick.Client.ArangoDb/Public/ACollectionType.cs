// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains the different type of collections.
/// </summary>
public enum ACollectionType
{
    /// <summary>
    ///     Document collection
    /// </summary>
    Document = 2,

    /// <summary>
    ///     Edge collection
    /// </summary>
    Edge = 3
}
