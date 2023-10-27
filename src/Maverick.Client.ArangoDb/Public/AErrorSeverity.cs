// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Severity of error.
/// </summary>
public enum AErrorSeverity
{
    Hint = 0,
    Warning = 1,
    Error = 2,
    Fatal = 3
}
