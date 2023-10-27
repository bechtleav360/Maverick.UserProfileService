using System;
using System.Net;
using System.Net.Http.Headers;
using Maverick.Client.ArangoDb.Protocol;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Contains the basis API response information.
/// </summary>
public abstract class BaseApiResponse : IApiResponse
{
    /// <summary>
    ///     The HTTP status code
    /// </summary>
    public HttpStatusCode Code { get; set; }

    /// <summary>
    ///     Object containing all debug information
    /// </summary>
    public DebugInfo DebugInfos { get; protected set; }

    /// <summary>
    ///     boolean flag to indicate whether an error occurred (true in this case)
    /// </summary>
    public bool Error => Exception != null;

    /// <summary>
    ///     Arango exception
    /// </summary>
    public Exception Exception { get; protected set; }

    /// <summary>
    ///     Original response as string
    /// </summary>
    public string ResponseBodyAsString { get; }

    /// <summary>
    ///     Response Headers
    /// </summary>
    public HttpResponseHeaders ResponseHeaders { get; protected set; }

    /// <summary>
    ///     Default constructor the BaseApiResponse
    /// </summary>
    internal BaseApiResponse()
    {
    }

    internal BaseApiResponse(Exception exception, DebugInfo debugInfos, HttpResponseHeaders responseHeaders)
    {
        Exception = exception;
        ResponseHeaders = responseHeaders;
        DebugInfos = debugInfos;
    }

    internal BaseApiResponse(Response clientResponse, Exception exception = null)
    {
        Code = clientResponse.StatusCode;
        DebugInfos = clientResponse.DebugInfo ?? new DebugInfo();
        Exception = exception;
        ResponseHeaders = clientResponse.ResponseHeaders;
        ResponseBodyAsString = clientResponse?.ResponseBodyAsString;
    }
}
