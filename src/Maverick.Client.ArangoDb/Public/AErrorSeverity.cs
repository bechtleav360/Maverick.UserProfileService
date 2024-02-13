// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Severity of error.
/// </summary>
public enum AErrorSeverity
{
    /// <summary>
    ///     Indicates a hint or informational message.
    /// </summary>
    Hint = 0,

    /// <summary>
    ///     Represents a warning message.
    /// </summary>
    Warning = 1,

    /// <summary>
    ///     Indicates an error condition.
    /// </summary>
    Error = 2,

    /// <summary>
    ///     Represents a fatal error.
    /// </summary>
    Fatal = 3
}
