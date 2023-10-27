using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using UserProfileService.Configuration;

namespace UserProfileService.Utilities;

internal class RolesAuthorizationRequirementHandler : RequirementHandlerBase<RolesAuthorizationRequirement>
{
    /// <inheritdoc />
    public RolesAuthorizationRequirementHandler(IOptionsSnapshot<IdentitySettings> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override Task HandleAsync(
        AuthorizationHandlerContext context,
        RolesAuthorizationRequirement requirement)
    {
        return requirement.HandleAsync(context);
    }
}
