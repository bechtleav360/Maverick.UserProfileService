using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Extensions;

public static class HttpContextExtension
{
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

        if (userId == null)
        {
            logger?.LogInfoMessage(
                "Current user is not authenticated. No user id in the request.",
                LogHelpers.Arguments());
        }

        return logger?.ExitMethod<string>(userId);
    }
}
