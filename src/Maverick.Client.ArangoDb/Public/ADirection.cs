// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Represents Edges directions.
/// </summary>
public enum ADirection
{
    /// <summary>
    ///     Reverse direction (inbound)
    /// </summary>
    In,

    /// <summary>
    ///     Forward direction (outbound)
    /// </summary>
    Out,

    /// <summary>
    ///     Forward and reverse direction
    /// </summary>
    Any
}
