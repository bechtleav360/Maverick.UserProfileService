using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Configuration;

public class ProfileDeputyConfiguration
{
    /// <summary>
    ///     Indicates the representatives for the different types of profiles. The identifier is the internal ID of the
    ///     profile.
    /// </summary>
    public Dictionary<RequestedProfileKind, string> Profiles { get; set; } =
        new Dictionary<RequestedProfileKind, string>();

    /// <summary>
    ///     Specifies whether the substitution rule is statically routed to the defined users (see <see cref="Profiles" />).
    /// </summary>
    public bool Static { get; set; } = true;
}
