using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Configuration;
using UserProfileService.Utilities;

namespace UserProfileService.Extensions;

internal static class ServiceCollectionAuthorizationExtensions
{
    /// <summary>
    ///     Provides a forwarding func for JWT vs reference tokens (based on existence of dot in token)
    /// </summary>
    /// <param name="httpCpContext">The http context that contains the reference token.</param>
    /// <param name="introspectionScheme">Scheme name of the introspection handler</param>
    /// <param name="userIdScheme">Scheme name of the UserID handler</param>
    /// <returns></returns>
    private static string ForwardReferenceToken(
        HttpContext httpCpContext,
        string introspectionScheme,
        string userIdScheme)
    {
        (string scheme, string credential) = GetSchemeAndCredential(httpCpContext);

        if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) && !credential.Contains("."))
        {
            return introspectionScheme;
        }

        return httpCpContext.Request.Headers.ContainsKey(WellKnownIdentitySettings.ImpersonateHeader)
            ? userIdScheme
            : null;
    }

    internal static void AddMaverickIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger = null)
    {
        var identitySettings = configuration.Get<IdentitySettings>();

        if (!identitySettings.EnableAuthorization)
        {
            logger.LogInfoMessage("The identity is disabled and won't be registered.", LogHelpers.Arguments());
            return;
        }
        
        services.AddAuthentication("token")
            .AddJwtBearer(
                "token",
                o =>
                {
                    o.Authority = identitySettings.GetAuthorityEndpointUriString();
                    o.Audience = identitySettings.JwtAccessTokenAudience;
                    o.RequireHttpsMetadata = identitySettings.RequireHttpsMetadata;
                    o.TokenValidationParameters.ValidateAudience = false;

                    if (!string.IsNullOrEmpty(identitySettings.ClaimsIssuer))
                    {
                        o.ClaimsIssuer = identitySettings.ClaimsIssuer;
                    }

                    o.SaveToken = identitySettings.SaveToken;
                    o.IncludeErrorDetails = identitySettings.IncludeErrorDetails;
                    o.Challenge = JwtBearerDefaults.AuthenticationScheme;
                    o.ForwardDefaultSelector = httpContext => ForwardReferenceToken(
                        httpContext,
                        "introspection",
                        "userid");
                })
            // reference tokens
            .AddOAuth2Introspection(
                "introspection",
                o =>
                {
                    o.Authority = identitySettings.GetAuthorityEndpointUriString();

                    o.CacheKeyPrefix =
                        identitySettings.CacheKeyPrefix;

                    o.CacheDuration =
                        TimeSpan.FromMinutes(identitySettings.CacheDurationInMinutes);

                    o.EnableCaching = identitySettings.EnableCaching;
                    o.ClientId = identitySettings.ApiName;
                    o.ClientSecret = identitySettings.ApiSecret;
                    o.NameClaimType = identitySettings.NameClaimType;
                    o.RoleClaimType = identitySettings.RoleClaimType;

                    if (!string.IsNullOrEmpty(identitySettings.ClaimsIssuer))
                    {
                        o.ClaimsIssuer = identitySettings.ClaimsIssuer;
                    }

                    o.SaveToken = identitySettings.SaveToken;
                })
            // userid header
            .AddScheme<UserIdHeaderAuthenticationHeaderOptions, UserIdHeaderAuthenticationHandler>(
                "userid",
                o => o.IsEnabled = identitySettings.EnableAnonymousImpersonation);

        logger?.LogInfoMessage(
            "Identity settings applied. Authorization check {authEnabled}. Configured authority endpoint: {endpoint}. Allow anonymous impersonation {impersonationEnabled}.",
            LogHelpers.Arguments(
                identitySettings.EnableAuthorization ? "enabled" : "disabled",
                identitySettings.Authority,
                identitySettings.EnableAnonymousImpersonation ? "enabled" : "disabled"));
    }

    internal static void AddSupportForAnonymousRequests(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger = null)
    {
        var identitySettings = configuration.Get<IdentitySettings>();

        if (!identitySettings.EnableAuthorization)
        {
            services.AddSingleton<IAuthorizationHandler, AllowAnonymousEverywhere>();
        }

        logger?.LogInfoMessage(
            "Identity settings applied. Allow anonymous requests {authEnabled}.",
            LogHelpers.Arguments(identitySettings.EnableAuthorization ? "disabled" : "enabled"));
    }

    /// <summary>
    ///     Extracts scheme and credential from Authorization header (if present)
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static (string, string) GetSchemeAndCredential(HttpContext context)
    {
        string header = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(header))
        {
            return ("", "");
        }

        string[] parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length != 2
            ? ("", "")
            : (parts[0], parts[1]);
    }

    /// <summary>
    ///     Returns the authority endpoint address stored in parameter <paramref name="identitySettings" />.
    ///     Default ports will be ignored in the output.
    /// </summary>
    /// <param name="identitySettings">The identity settings object that contains the authority address.</param>
    /// <returns>The endpoint address as string.</returns>
    /// <exception cref="System.Exception">
    ///     If identity settings is <c>null</c>. -or-<br />
    ///     If authority endpoint is null or an empty string or only whitespace. -or-<br />
    ///     If authority endpoint cannot be converted to an Uri object.
    /// </exception>
    public static string GetAuthorityEndpointUriString(this IdentitySettings identitySettings)
    {
        // These exceptions will be returned in the applications log.
        // That's why ArgumentExceptions have been avoided in this context.
        if (identitySettings == null)
        {
            throw new Exception("Could not find any identity settings in configuration.");
        }

        if (string.IsNullOrWhiteSpace(identitySettings.Authority))
        {
            throw new Exception("Authority is not defined in configured identity settings.");
        }

        if (!Uri.TryCreate(identitySettings.Authority, UriKind.Absolute, out Uri authorityEndpoint))
        {
            throw new Exception("The configured authority is not a valid endpoint address (see identity settings).");
        }

        var uriBuilder = new UriBuilder(authorityEndpoint);

        if (authorityEndpoint.IsDefaultPort)
        {
            uriBuilder.Port = -1;
        }

        return uriBuilder.ToString().TrimEnd('/');
    }
}
