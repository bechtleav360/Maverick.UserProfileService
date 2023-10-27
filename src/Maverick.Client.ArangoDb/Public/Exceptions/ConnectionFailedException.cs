using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Maverick.Client.ArangoDb.Public.Exceptions;

/// <summary>
///     Represents errors that occur during establishing a connection to the ArangoDb server.
/// </summary>
public class ConnectionFailedException : ApiErrorException
{
    /// <summary>
    ///     A list of endpoint addresses, that has been used, when the exception occurred.
    /// </summary>
    public IList<string> Endpoints { get; set; }

    /// <summary>
    ///     The URI string that has been used combined with <see cref="UsedHttpMethod" />.
    /// </summary>
    public string OperationUri { get; set; }

    /// <summary>
    ///     The HTTP method that has been handled when the error occurred.
    /// </summary>
    public string UsedHttpMethod { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ConnectionFailedException" /> with default values for all properties.
    /// </summary>
    public ConnectionFailedException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ConnectionFailedException" /> with specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConnectionFailedException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ConnectionFailedException" /> with specified error message and used
    ///     endpoint connections.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="endpoints">The endpoint that has been tried to establish a connection to ArangoDb.</param>
    public ConnectionFailedException(
        string message,
        IEnumerable<string> endpoints)
        : base(message)
    {
        Endpoints = endpoints?.ToList();
    }

    /// <inheritdoc cref="Exception" />
    [SecurityCritical]
    public override string ToString()
    {
        var s = $"{GetType().Name}: {Message}";

        if (!string.IsNullOrWhiteSpace(UsedHttpMethod) && !string.IsNullOrWhiteSpace(OperationUri))
        {
            s += $"{Environment.NewLine}while HTTP operation: {UsedHttpMethod.ToUpper()} {OperationUri}";
        }

        if (InnerException != null)
        {
            s += $" ---> {InnerException}";
        }

        if (StackTrace != null)
        {
            s += Environment.NewLine + StackTrace;
        }

        return s;
    }

    /// <summary>
    ///     Get severity of current exception.
    /// </summary>
    /// <returns>
    ///     <see cref="AErrorSeverity" />
    /// </returns>
    public override AErrorSeverity GetSeverity()
    {
        return AErrorSeverity.Fatal;
    }
}
