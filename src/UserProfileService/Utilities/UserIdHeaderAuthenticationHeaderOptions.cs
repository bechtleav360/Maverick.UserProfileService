using Microsoft.AspNetCore.Authentication;

namespace UserProfileService.Utilities;

/// <summary>
///     Options for <see cref="UserIdHeaderAuthenticationHandler" />.
/// </summary>
public class UserIdHeaderAuthenticationHeaderOptions : AuthenticationSchemeOptions
{
    /// <summary>
    ///     Specifies whether a request with the X-Userid header should count as authorized.
    /// </summary>
    public bool IsEnabled { get; set; }
}
