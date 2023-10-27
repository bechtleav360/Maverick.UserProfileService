using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UserProfileService.Configuration;

namespace UserProfileService.Utilities;

internal abstract class RequirementHandlerBase<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
{
    private readonly IdentitySettings _settings;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    protected RequirementHandlerBase(IOptionsSnapshot<IdentitySettings> options)
    {
        _settings = options.Value;
    }

    protected sealed override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
    {
        if (!_settings.EnableAuthorization)
        {
            context.Succeed(requirement);

            return Task.FromResult(true);
        }

        return HandleAsync(context, requirement);
    }

    protected abstract Task HandleAsync(AuthorizationHandlerContext context, T requirement);
}
