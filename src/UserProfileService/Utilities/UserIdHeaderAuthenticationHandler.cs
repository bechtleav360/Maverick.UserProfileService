using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Configuration;

namespace UserProfileService.Utilities;

/// <summary>
///     Implements an <see cref="AuthenticationHandler{TOptions}" /> in order to allow unauthenticated use of the X-UserId
///     header.
/// </summary>
public class UserIdHeaderAuthenticationHandler : AuthenticationHandler<UserIdHeaderAuthenticationHeaderOptions>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="UserIdHeaderAuthenticationHandler" />
    /// </summary>
    /// <param name="options">Contains the options used in order to configure the authentication handler.</param>
    /// <param name="logger">Specifies the logger to use.</param>
    /// <param name="encoder">A <see cref="UrlEncoder" />.</param>
    /// <param name="clock">A <see cref="ISystemClock" />.</param>
    public UserIdHeaderAuthenticationHandler(
        IOptionsMonitor<UserIdHeaderAuthenticationHeaderOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.EnterMethod();

        if (!Options.IsEnabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.ContainsKey(WellKnownIdentitySettings.ImpersonateHeader))
        {
            Logger.LogDebugMessage(
                "The request does not contain the header {userIdHeader}.",
                LogHelpers.Arguments(WellKnownIdentitySettings.ImpersonateHeader));

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers[WellKnownIdentitySettings.ImpersonateHeader].ToString();

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(WellKnownIdentitySettings.DefaultUserIdClaim, userId) },
                    nameof(UserIdHeaderAuthenticationHandler))),
            Scheme.Name);

        Logger.ExitMethod();

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
