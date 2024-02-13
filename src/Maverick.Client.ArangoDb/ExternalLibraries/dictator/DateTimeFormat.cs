// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents different formats for date and time values.
/// </summary>
public enum DateTimeFormat
{
    /// <summary>
    ///     Specifies that the value is an object representing date and time.
    /// </summary>
    Object,

    /// <summary>
    ///     Specifies that the value is a string representation of date and time.
    /// </summary>
    String,

    /// <summary>
    ///     Specifies that the value is a Unix timestamp (numeric representation of seconds since January 1, 1970).
    /// </summary>
    UnixTimeStamp
}