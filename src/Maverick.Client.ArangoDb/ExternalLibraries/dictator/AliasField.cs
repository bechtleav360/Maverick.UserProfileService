using System;


// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Specified alias will be used as field name to convert property to or from document format.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AliasField : Attribute
{
    public string Alias { get; set; }

    public AliasField(string alias)
    {
        Alias = alias;
    }
}
