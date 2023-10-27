using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

public sealed class DataSetSchema
{
    public List<string> Info; //{ get; set; }
    public string Name; //{ get; set; }
}
