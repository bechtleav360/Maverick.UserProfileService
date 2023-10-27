// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Defines the type of the response.
/// </summary>
public enum AResponseType
{
    /// <summary>
    ///     All except cursor responses.
    /// </summary>
    Other,

    /// <summary>
    ///     Cursor response (but not the first response (cursor created))
    /// </summary>
    Cursor,

    /// <summary>
    ///     Cursor response and the first response of the multi api response.
    /// </summary>
    CursorFirstResponse
}
