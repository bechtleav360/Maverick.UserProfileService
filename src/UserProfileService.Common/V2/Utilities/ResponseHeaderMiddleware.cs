using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     This middleware add the correlation id in the header of the response.
/// </summary>
public class ResponseHeaderMiddleware
{
    private readonly ILogger<ResponseHeaderMiddleware> _Logger;
    private readonly RequestDelegate _Next;

    /// <summary>
    ///     The constructor that is used to create the object <see cref="ResponseHeaderMiddleware" />.
    /// </summary>
    /// <param name="next">The request delegate to trigger the next middleware.</param>
    /// <param name="logger">The logger that is used to log important or useful messages.</param>
    public ResponseHeaderMiddleware(RequestDelegate next, ILogger<ResponseHeaderMiddleware> logger)
    {
        _Next = next;
        _Logger = logger;
    }

    /// <summary>
    ///     This method is use to add the correlation id in the header information of the response.
    /// </summary>
    /// <param name="context">The http context where the header information is stored.</param>
    /// <returns> A task representing the asynchronous write operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = Activity.Current?.Id;

        if (!string.IsNullOrEmpty(correlationId))
        {
            _Logger.LogInfoMessage(
                "Adding correlationId: {correlationId} to the header name {headerName}.",
                LogHelpers.Arguments(correlationId, "x-Correlation-Id"));

            context.Response.Headers.Add("x-Correlation-Id", correlationId);
        }

        await _Next(context);
    }
}
