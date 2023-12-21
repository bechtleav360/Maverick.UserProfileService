using System;

namespace Maverick.Client.ArangoDb.Public.Exceptions;

/// <summary>
///     Represents an exception that is thrown when JSON deserialization fails, including issues with
///     missing or malformed JSON strings, and other deserialization errors.
/// </summary>
public class JsonDeserializationException : Exception
{
    /// <summary>
    ///     Gets the original JSON string that caused the deserialization exception.
    /// </summary>
    public string OriginalJsonString { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonDeserializationException" /> class
    ///     with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    public JsonDeserializationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonDeserializationException" /> class
    ///     with a specified error message and the original JSON string.
    /// </summary>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    /// <param name="originalJsonString">The original JSON string that caused the deserialization exception.</param>
    public JsonDeserializationException(string message, string originalJsonString)
        : base(message)
    {
        OriginalJsonString = originalJsonString;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonDeserializationException" /> class
    ///     with a specified error message, the original JSON string, and a reference to the inner exception
    ///     that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    /// <param name="originalJsonString">The original JSON string that caused the deserialization exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference
    ///     if no inner exception is specified.
    /// </param>
    public JsonDeserializationException(string message, string originalJsonString, Exception innerException)
        : base(message, innerException)
    {
        OriginalJsonString = originalJsonString;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{base.ToString()}{Environment.NewLine}Used JSON string:{Environment.NewLine}{OriginalJsonString}";
    }
}
