using IdentityModel;

namespace UserProfileService.Api.Common.Configuration;

/// <summary>
///     Contains default values included in <see cref="IdentitySettings" />.
/// </summary>
public static class WellKnownIdentitySettings
{
    /// <summary>
    ///     Specifies the default claim type to use for the name claim.
    /// </summary>
    public const string DefaultNameClaimType = "name";

    /// <summary>
    ///     Specifies the default claim type to use for the role claim.
    /// </summary>
    public const string DefaultRoleClaimType = "role";

    /// <summary>
    ///     Specifies the claim which contains the userid.
    /// </summary>
    public const string DefaultUserIdClaim = JwtClaimTypes.Subject;

    /// <summary>
    ///     Specifies the name of the herder required to impersonate as somebody.
    /// </summary>
    public const string ImpersonateHeader = "X-UserId";

    /// <summary>
    ///     The default scope name of OpenId.
    /// </summary>
    public const string OpenIdStandardScope = "openid";
}
