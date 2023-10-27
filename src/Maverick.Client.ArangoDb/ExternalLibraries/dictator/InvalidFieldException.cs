using System;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

public class InvalidFieldException : Exception
{
    public InvalidFieldException(string message) : base(message)
    {
    }
}
