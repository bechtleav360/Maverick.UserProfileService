using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Saga.Worker.Utilities;

/// <summary>
///     The profile extension contains methods to convert <see cref="ProfileKind" />
///     to <see cref="RequestedProfileKind" /> or <see cref="ProfileContainerType" />.
/// </summary>
public static class ProfileKindExtension
{
    /// <summary>
    ///     The method transforms the <see cref="ProfileKind" /> to a <see cref="RequestedProfileKind" />.
    /// </summary>
    /// <param name="profileKind">The profile kind that should be transformed.</param>
    /// <returns>A <see cref="RequestedProfileKind" /> that was transformed from the <paramref name="profileKind" /> />.</returns>
    public static RequestedProfileKind ConvertToRequestedProfileKind(this ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => RequestedProfileKind.Group,
            ProfileKind.User => RequestedProfileKind.User,
            ProfileKind.Unknown => RequestedProfileKind.All,
            _ => RequestedProfileKind.Undefined
        };
    }

    /// <summary>
    ///     Transforms the <see cref="ProfileKind" /> to a <see cref="ProfileContainerType" />.
    /// </summary>
    /// <param name="profileKind">The profile kind that should be transformed.</param>
    /// <returns>A <see cref="ProfileContainerType" /> that was transformed from the <paramref name="profileKind" /></returns>
    public static ProfileContainerType ToContainerProfileType(this ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => ProfileContainerType.Group,
            ProfileKind.Organization => ProfileContainerType.Organization,
            _ => ProfileContainerType.NotSpecified
        };
    }
}
