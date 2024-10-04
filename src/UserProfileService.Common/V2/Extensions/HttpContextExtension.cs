using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Extension class containing extension methods for <see cref="HttpContext"/>.
/// </summary>
public static class HttpContextExtension
{
    /// <summary>
    ///     Extracts the user id from the <paramref name="context"/>
    ///     by checking its "X-UserId" header and the user claim of type "sub" for a user id.
    /// </summary>
    /// <param name="context">The context to check.</param>
    /// <param name="logger">An optional logger.</param>
    /// <returns>The user id if it was found, <see langword="null"/> otherwise.</returns>
    public static string GetUserId(this HttpContext context, ILogger logger = null)
    {
        string userId = default;

        if (context.Request.Headers.TryGetValue(
                "X-UserId",
                out StringValues headerValues))
        {
            userId = headerValues.FirstOrDefault();
            logger?.LogDebugMessage("Received user id from header: {externalId}.", LogHelpers.Arguments(userId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = context.User?.Claims
                            ?.FirstOrDefault(c => c.Type == "sub")
                            ?.Value;

            logger?.LogDebugMessage("Received user id from token: {externalId}.", LogHelpers.Arguments(userId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                            ?.Value;

            logger?.LogDebugMessage(
                "Received user id from token via {claim} claim: {externalId}.",
                LogHelpers.Arguments(ClaimTypes.NameIdentifier, userId));
        }

        if (userId == null)
        {
            logger?.LogInfoMessage(
                "Current user is not authenticated. No user id in the request.",
                LogHelpers.Arguments());
        }

        return logger?.ExitMethod<string>(userId);
    }
}
