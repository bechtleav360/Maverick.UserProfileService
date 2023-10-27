using System;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Ignores property if it contains null value when converting object to or from document format.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreNullValue : Attribute
{
}
