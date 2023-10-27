using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using UserProfileService.Configuration;

namespace UserProfileService.Utilities;

internal class
    DenyAnonymousAuthorizationRequirementHandler : RequirementHandlerBase<DenyAnonymousAuthorizationRequirement>
{
    /// <inheritdoc />
    public DenyAnonymousAuthorizationRequirementHandler(IOptionsSnapshot<IdentitySettings> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override Task HandleAsync(
        AuthorizationHandlerContext context,
        DenyAnonymousAuthorizationRequirement requirement)
    {
        ClaimsPrincipal user = context.User;

        bool userIsAnonymous =
            user.Identity == null || !user.Identities.Any(i => i.IsAuthenticated);

        if (!userIsAnonymous)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
