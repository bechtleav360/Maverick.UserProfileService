using System;
using System.Security;
using Maverick.Client.ArangoDb.Protocol.Extensions;
using Maverick.Client.ArangoDb.Public.Annotations;

namespace Maverick.Client.ArangoDb.Public.Exceptions;

/// <summary>
///     Exception class for ArangoDB specific exceptions.
/// </summary>
[Serializable]
public class ApiErrorException : Exception
{
    /// <summary>
    ///     Indicates whether the error is an arango specified error and was actively returned by arango.
    /// </summary>
    public bool AError { get; set; }

    /// <summary>
    ///     ArangoDB error number.
    ///     See https://www.arangodb.com/docs/stable/appendix-error-codes.html for error numbers and descriptions.
    /// </summary>
    public AStatusCode? AErrorNum { get; set; }

    /// <summary>
    ///     Error message returned by arango.
    /// </summary>
    public string AMessage { get; set; }

    /// <summary>
    ///     Default constructor for ApiErrorException.
    /// </summary>
    public ApiErrorException()
    {
    }

    /// <summary>
    ///     Initialize the ApiErrorException with an error message
    /// </summary>
    /// <param name="message"></param>
    public ApiErrorException(string message) : base(message)
    {
        AError = false;
    }

    /// <summary>
    ///     Initialize the ApiErrorException with an from API Error object
    /// </summary>
    /// <param name="error">
    ///     Object containing some infos about an ArangoDB specific error <see cref="ArangoErrorResponse" />
    /// </param>
    public ApiErrorException(ArangoErrorResponse error) : base(error?.ErrorMessage ?? "Error message: <empty>")
    {
        AError = error?.Error ?? false;
        AErrorNum = error?.ErrorNum;
        AMessage = error?.ErrorMessage;
    }

    /// <summary>
    ///     Initialize the ApiErrorException with an from API Error object
    /// </summary>
    /// <param name="message"></param>
    /// <param name="error">
    ///     Object containing some infos about an ArangoDB specific error <see cref="ArangoErrorResponse" />
    /// </param>
    public ApiErrorException(string message, ArangoErrorResponse error) : base(message)
    {
        AError = error?.Error ?? false;
        AErrorNum = error?.ErrorNum;
        AMessage = error?.ErrorMessage;
    }

    /// <summary>
    ///     Initialize ApiErrorException with the error message and some thrown exception
    /// </summary>
    /// <param name="message">Error Message</param>
    /// <param name="innerException">Generic Exception</param>
    public ApiErrorException(string message, Exception innerException) : base(message, innerException)
    {
        AError = false;
    }

    private string AErrorToString()
    {
        return AError ? $"Error: Code: {AErrorNum}; Message: {AMessage}" : null;
    }

    /// <inheritdoc />
    [SecurityCritical]
    public override string ToString()
    {
        string s;

        if (Message == null || Message.Length <= 0)
        {
            s = nameof(ApiErrorException);
        }
        else
        {
            s = nameof(ApiErrorException) + ": " + Message;
        }

        if (AError)
        {
            s = s + Environment.NewLine + "Api error information: " + AErrorToString();
        }

        if (InnerException != null)
        {
            s = s + " ---> " + InnerException + Environment.NewLine + "   ";
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
    public virtual AErrorSeverity GetSeverity()
    {
        return AErrorNum?.GetAttributeOfType<ErrorClassificationAttribute>()?.Severity ?? AErrorSeverity.Error;
    }
}
