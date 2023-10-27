using System;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Contains extension methods related to <see cref="ProfileKind" />
///     .
/// </summary>
public static class ProfileKindExtension
{
    /// <summary>
    ///     Mapped the given profile kind to object type.
    /// </summary>
    /// <param name="profileKind">Profile kind to be mapped.</param>
    /// <returns>Mapped object type of profile kind.</returns>
    public static ObjectType ToObjectType(this ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => ObjectType.Group,
            ProfileKind.Organization => ObjectType.Organization,
            ProfileKind.User => ObjectType.User,
            ProfileKind.Unknown => ObjectType.Profile,
            _ => throw new ArgumentOutOfRangeException(
                nameof(profileKind),
                $"{nameof(ProfileKind)} could not be mapped to {nameof(ObjectType)}.")
        };
    }
}
