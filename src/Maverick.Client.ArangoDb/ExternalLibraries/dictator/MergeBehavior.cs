// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents different merge behaviors for combining fields.
/// </summary>
public enum MergeBehavior
{
    /// <summary>
    ///     Specifies that fields should be overwritten during merging.
    /// </summary>
    OverwriteFields,

    /// <summary>
    ///     Specifies that existing fields should be kept during merging.
    /// </summary>
    KeepFields
}