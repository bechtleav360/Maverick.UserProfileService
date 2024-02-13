// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents different constraints for validation.
/// </summary>
public enum Constraint
{
    /// <summary>
    ///     Specifies that a value must be present.
    /// </summary>
    MustHave,

    /// <summary>
    ///     Suggests that a value should be present but is not mandatory.
    /// </summary>
    ShouldHave,

    /// <summary>
    ///     Indicates that a value cannot be null.
    /// </summary>
    NotNull,

    /// <summary>
    ///     Describes a specific data type constraint.
    /// </summary>
    Type,

    /// <summary>
    ///     Defines a minimum value constraint.
    /// </summary>
    Min,

    /// <summary>
    ///     Defines a maximum value constraint.
    /// </summary>
    Max,

    /// <summary>
    ///     Specifies a range constraint.
    /// </summary>
    Range,

    /// <summary>
    ///     Indicates a size constraint (e.g., string length, collection size).
    /// </summary>
    Size,

    /// <summary>
    ///     Describes a pattern matching constraint.
    /// </summary>
    Match
}