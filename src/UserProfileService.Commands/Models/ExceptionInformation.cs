using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UserProfileService.Commands.Models;

/// <summary>
///     Contains information about a occurred exception (serialize safely)
/// </summary>
public class ExceptionInformation
{
    /// <summary>
    ///     Gets a collection of key/value pairs that provide additional user-defined information about the exception.
    /// </summary>
    public Dictionary<object, object> Data { get; } = new Dictionary<object, object>();

    /// <summary>
    ///     The name of the type of the original occurred exception.
    /// </summary>
    public string ExceptionType { get; set; }

    /// <summary>
    ///     Gets or sets a link to the help file associated with this exception.
    /// </summary>
    public string HelpLink { get; set; }

    /// <summary>
    ///     Gets or sets HRESULT, a coded numerical value that is assigned to a specific exception.
    /// </summary>
    public int HResult { get; set; }

    /// <summary>
    ///     Gets the <see cref="ExceptionInformation" /> instance that caused the current exception.
    /// </summary>
    public ExceptionInformation InnerException { get; set; }

    /// <summary>
    ///     Gets a message that describes the current exception.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Gets or sets the name of the application or the object that causes the error.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Gets a string representation of the immediate frames on the call stack.
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ExceptionInformation" />.
    /// </summary>
    public ExceptionInformation()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ExceptionInformation" />.
    /// </summary>
    [JsonConstructor]
    public ExceptionInformation(
        string message,
        string stackTrace,
        string exceptionType,
        string source,
        int hResult,
        string helpLink,
        Dictionary<object, object> data,
        ExceptionInformation innerException)
    {
        Message = message;
        StackTrace = stackTrace;
        ExceptionType = exceptionType;
        Source = source;
        HResult = hResult;
        HelpLink = helpLink;
        InnerException = innerException;
        Data = data;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ExceptionInformation" />.
    /// </summary>
    public ExceptionInformation(Exception exception)
    {
        ExceptionType = exception.GetType().Name;
        Source = exception.Source;
        Message = exception.Message;
        StackTrace = exception.StackTrace;
        HResult = exception.HResult;
        HelpLink = exception.HelpLink;

        foreach (DictionaryEntry entry in exception.Data)
        {
            if (entry.Value == null
                || !entry.Value.GetType().IsPrimitive
                || entry.Value is not DateTime
                || entry.Value is not string)
            {
                continue;
            }

            Data.TryAdd(entry.Key, entry.Value);
        }

        if (exception.InnerException == null)
        {
            return;
        }

        InnerException = new ExceptionInformation(exception.InnerException);
    }
}
