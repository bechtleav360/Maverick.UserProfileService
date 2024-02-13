// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents different formats for enumerations.
/// </summary>
public enum EnumFormat
{
    /// <summary>
    ///     Specifies that the value is an object representation.
    /// </summary>
    Object,

    /// <summary>
    ///     Specifies that the value is an integer representation.
    /// </summary>
    Integer,

    /// <summary>
    ///     Specifies that the value is a string representation.
    /// </summary>
    String
}