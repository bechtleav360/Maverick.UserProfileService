// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Marks a response as cursor result.
/// </summary>
public interface ICursorResponse
{
    /// <summary>
    ///     Gets detailed information about the cursor response.
    /// </summary>
    ICursorInnerResponse CursorDetails { get; }
}
