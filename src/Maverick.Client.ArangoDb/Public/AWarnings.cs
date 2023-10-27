// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains Warnings
/// </summary>
public class AWarnings
{
    /// <summary>
    ///     Http Statuscode
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    ///     Error message
    /// </summary>
    public string Message { get; set; }
}
