using UserProfileService.Api.Common.Configuration;

namespace UserProfileService.Configuration;

/// <summary>
///     Options for IdentityServer authentication
/// </summary>
public class IdentitySettings
{
    /// <summary>
    ///     Name of the API resource used for authentication against introspection endpoint
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    ///     Secret used for authentication against introspection endpoint
    /// </summary>
    public string ApiSecret { get; set; }

    /// <summary>
    ///     Base address of the token issuer.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    ///     Specifies ttl for introspection response caches. Default: 5
    /// </summary>
    public int CacheDurationInMinutes { get; set; } = 5;

    /// <summary>
    ///     Specifies the prefix of the cache key (token).
    /// </summary>
    public string CacheKeyPrefix { get; set; }

    /// <summary>
    ///     Gets or sets the issuer that should be used for any claims that are created
    /// </summary>
    public string ClaimsIssuer { get; set; }

    /// <summary>
    ///     Specifies whether unauthorized requests should be able to use the X-UserId header in order to impersonate a user.
    /// </summary>
    public bool EnableAnonymousImpersonation { get; set; } = false;

    /// <summary>
    ///     A boolean value indicating whether authorization should be enabled or not. Default: true
    /// </summary>
    public bool EnableAuthorization { get; set; } = true;

    /// <summary>
    ///     Specifies whether caching is enabled for introspection responses (requires a distributed cache implementation)
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    ///     Defines whether the token validation errors should be returned to the caller.
    ///     Enabled by default, this option can be disabled to prevent the JWT handler
    ///     from returning an error and an error_description in the WWW-Authenticate header.
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = true;

    /// <summary>
    ///     A single valid audience value for any received OpenIdConnect token. Default: openid <br />
    ///     If you are using API resources, you can specify the name here
    /// </summary>
    public string JwtAccessTokenAudience { get; set; } = WellKnownIdentitySettings.OpenIdStandardScope;

    /// <summary>
    ///     Specifies the claim type to use for the name claim. Default: name
    /// </summary>
    public string NameClaimType { get; set; } = WellKnownIdentitySettings.DefaultNameClaimType;

    /// <summary>
    ///     Specifies whether HTTPS is required for the discovery endpoint
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    ///     Specifies the claim type to use for the role claim Default: role
    /// </summary>
    public string RoleClaimType { get; set; } = WellKnownIdentitySettings.DefaultRoleClaimType;

    /// <summary>
    ///     Specifies whether the token should be saved in the authentication properties
    /// </summary>
    public bool SaveToken { get; set; } = true;
}
