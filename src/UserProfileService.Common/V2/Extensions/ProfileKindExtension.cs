using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Common.V2.Extensions;

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
                $"{nameof(ProfileKind)} could not be mapped to {nameof(ObjectType)}.")
        };
    }

    /// <summary>
    ///     Mapped the given profile kind to container type.
    /// </summary>
    /// <param name="profileKind">Profile kind to be mapped.</param>
    /// <returns>Mapped object type of profile kind.</returns>
    public static ContainerType ToContainerType(this ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => ContainerType.Group,
            ProfileKind.Organization => ContainerType.Organization,
            _ => throw new ArgumentOutOfRangeException(
                $"{nameof(ProfileKind)} could not be mapped to {nameof(ObjectType)}. Incoming profile kind : {profileKind} could not be mapped to a {nameof(ContainerType)}.")
        };
    }

    /// <summary>
    ///     Transforms a <see cref="ProfileIdent" /> to an <see cref="ObjectIdent" />.
    /// </summary>
    /// <param name="profileIdent">The <see cref="ProfileIdent" /> that should be transformed.</param>
    /// <returns>An <see cref="ObjectIdent" /> that was transformed.</returns>
    public static ObjectIdent ToObjectIdent(this ProfileIdent profileIdent)
    {
        return new ObjectIdent(profileIdent.Id, profileIdent.ProfileKind.ToObjectType());
    }
}
