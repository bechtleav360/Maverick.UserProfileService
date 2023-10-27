using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.EnumModels;
using AggregateObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="ObjectType" />
///     s.
/// </summary>
public static class ObjectTypeExtension
{
    /// <summary>
    ///     Checks if the given object type represents a profile.
    /// </summary>
    /// <param name="objectType">Object type to be checked.</param>
    /// <returns><see langword="true" /> if the type represents a profile; otherwise, <see langword="false" />.</returns>
    public static bool IsProfileType(this ObjectType objectType)
    {
        return objectType == ObjectType.Group
            || objectType == ObjectType.User
            || objectType == ObjectType.Organization
            || objectType == ObjectType.Profile;
    }

    /// <summary>
    ///     Checks if the given object type represents a profile.
    /// </summary>
    /// <param name="objectType">Object type to be checked.</param>
    /// <returns><see langword="true" /> if the type represents a profile; otherwise, <see langword="false" />.</returns>
    public static bool IsProfileType(this AggregateObjectType objectType)
    {
        return objectType == AggregateObjectType.Group
            || objectType == AggregateObjectType.User
            || objectType == AggregateObjectType.Organization
            || objectType == AggregateObjectType.Profile;
    }

    /// <summary>
    ///     Checks if the given object type represents a container profile.
    /// </summary>
    /// <param name="objectType">Object type to be checked.</param>
    /// <returns><see langword="true" /> if the type represents a profile; otherwise, <see langword="false" />.</returns>
    public static bool IsContainerProfileType(this ObjectType objectType)
    {
        return objectType == ObjectType.Group || objectType == ObjectType.Organization;
    }

    public static ContainerType ToContainerType(this ObjectType objectType)
    {
        return objectType switch
        {
            ObjectType.Group => ContainerType.Group,
            ObjectType.Organization => ContainerType.Organization,
            ObjectType.Function => ContainerType.Function,
            ObjectType.Role => ContainerType.Role,
            ObjectType.User => ContainerType.NotSpecified,
            _ => throw new ArgumentOutOfRangeException(
                nameof(objectType),
                objectType,
                $"{nameof(ObjectType)} '{objectType:G}' could not be mapped to {nameof(ContainerType)}.")
        };
    }
}
