using Microsoft.AspNetCore.Authorization;

namespace UserProfileService.Utilities;

/// <summary>
///     A helper-class to allow all anonymous requests in dev-environments.
/// </summary>
public class AllowAnonymousEverywhere : IAuthorizationHandler
{
    /// <inheritdoc cref="IAuthorizationHandler.HandleAsync" />
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (IAuthorizationRequirement requirement in context.PendingRequirements.ToList())
        {
            if (context.User.Identity is not
                {
                    IsAuthenticated: true
                })
            {
                // only allow unauthenticated users to do everything in oder to be able to test with user-permissions
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
