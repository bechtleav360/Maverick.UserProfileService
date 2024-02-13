using System;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.dictator;

/// <summary>
///     Represents an exception that occurs when an invalid field is accessed or referenced.
/// </summary>
public class NonExistingFieldException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NonExistingFieldException"/> class with the specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public NonExistingFieldException(string message) : base(message)
    {
    }
}
