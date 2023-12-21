using System;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Exceptions;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Standard Response Typ for public methods containing some debug information and a Value returned from API in field
///     (Result)
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleApiResponse<T> : SingleApiResponse, IResponseWithParsingException
{
    /// <summary>
    ///     Value contains in the API Response,
    /// </summary>
    public T Result { get; }

    /// <inheritdoc cref="IResponseWithParsingException.ParsingException"/>
    public JsonDeserializationException ParsingException { get; }

    internal SingleApiResponse(Response clientResponse, T originalResponse, Exception exception = null) : base(
        clientResponse,
        exception)
    {
        Result = originalResponse;
    }

    internal SingleApiResponse(Response clientResponse, Exception exception) : base(clientResponse, exception)
    {
    }

    internal SingleApiResponse(Response clientResponse, JsonDeserializationException exception) : base(clientResponse)
    {
        ParsingException = exception;
    }
}

/// <summary>
///     Standard Response Typ for public methods containing some request and response information.
/// </summary>
/// <inheritdoc />
public class SingleApiResponse : BaseApiResponse
{
    internal SingleApiResponse(Response clientResponse, Exception exception = null) : base(
        clientResponse,
        exception)
    {
    }
}
