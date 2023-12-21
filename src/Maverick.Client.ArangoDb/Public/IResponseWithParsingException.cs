using Maverick.Client.ArangoDb.Public.Exceptions;

namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Represents an interface implemented by response models that may contain
///     properties related to parsing or deserialization exceptions.
/// </summary>
public interface IResponseWithParsingException
{
    /// <summary>
    ///     Gets or sets the exception that occurred during parsing or deserialization.
    ///     This property provides detailed information about any issues encountered
    ///     while processing and interpreting data.
    /// </summary>
    JsonDeserializationException ParsingException { get; }
}
