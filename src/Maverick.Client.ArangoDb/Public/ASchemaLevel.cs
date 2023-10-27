using System.Runtime.Serialization;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains possible levels of a JSON schema validation. The level controls when the validation is triggered:
/// </summary>
public enum ASchemaLevel
{
    /// <summary>
    ///     All new and modified document must strictly pass validation. No exceptions are made (default).
    /// </summary>
    [EnumMember(Value = "strict")]
    Strict,

    /// <summary>
    ///     The rule is inactive and validation thus turned off.
    /// </summary>
    [EnumMember(Value = "none")]
    None,

    /// <summary>
    ///     Only newly inserted documents are validated.
    /// </summary>
    [EnumMember(Value = "new")]
    New,

    /// <summary>
    ///     New and modified documents must pass validation, except for modified documents where the OLD value did not pass
    ///     validation already. This level is useful if you have documents which do not match your target structure, but you
    ///     want to stop the insertion of more invalid documents and prohibit that valid documents are changed to invalid
    ///     documents.
    /// </summary>
    [EnumMember(Value = "moderate")]
    Moderate
}
