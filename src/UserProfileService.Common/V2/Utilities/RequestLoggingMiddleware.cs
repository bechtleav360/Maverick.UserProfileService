using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     The request middleware logs the incoming message parameter when a controller method is called.
///     Sensitive data will be only logged, when trace mode is activated.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _Logger;
    private readonly RequestDelegate _Next;

    /// <summary>
    ///     The constructor that is used to create the object <see cref="RequestLoggingMiddleware" />.
    /// </summary>
    /// <param name="next">The request delegate to trigger the next middleware.</param>
    /// <param name="logger">The logger that is used to log important or useful messages.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _Next = next;
        _Logger = logger;
    }

    /// <summary>
    ///     This method is used to log information about the incoming request.
    /// </summary>
    /// <param name="context">The http context where the parameter of the request are stored.</param>
    /// <returns> A task representing the asynchronous write operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        _Logger.LogInfoMessage(
            "Used requestUrl: {requestUrl}, with Method: {requestMethod}, and query parameter: {requestQueryString}.",
            LogHelpers.Arguments(
                context.Request?.Path,
                context.Request?.Method,
                context.Request?.QueryString));

        _Logger.LogDebugMessage(
            "Receiving http request with schema: {requestSchema}://{hostUrl}",
            LogHelpers.Arguments(
                context.Request?.Scheme,
                context.Request?.Host));

        if (_Logger.IsEnabled(LogLevel.Trace) && context.Request != null)
        {
            var requestBody = new StreamReader(context.Request?.Body);
            string requestBodyContent = await requestBody.ReadToEndAsync();
            _Logger.LogTraceMessage("RequestBody: {requestBody}.", LogHelpers.Arguments(requestBodyContent));
        }

        if (context.Request != null)
        {
            context.Request.Body.Position = 0;
        }

        await _Next(context);

        _Logger.LogInfoMessage(
            "Responding with status code {statusCode} and content length: {contentLength} bytes.",
            LogHelpers.Arguments(context.Response.StatusCode, context.Response.ContentLength));
    }
}
