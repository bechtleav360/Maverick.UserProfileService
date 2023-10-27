using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.EnumModels;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains methods to convert model classes or elements of models.
/// </summary>
public static class ConversionExtensions
{
    /// <summary>
    ///     Converts <see cref="RequestedProfileKind" /> to its
    ///     <see cref="ProfileKind" /> representation.
    /// </summary>
    /// <param name="requestedProfileKind">The <see cref="RequestedProfileKind" /> to be converted.</param>
    /// <returns>
    ///     The suitable <see cref="ProfileKind" /> of
    ///     <paramref name="requestedProfileKind" />.
    /// </returns>
    public static ProfileKind Convert(this RequestedProfileKind requestedProfileKind)
    {
        return requestedProfileKind switch
        {
            RequestedProfileKind.Group => ProfileKind.Group,
            RequestedProfileKind.User => ProfileKind.User,
            RequestedProfileKind.Organization => ProfileKind.Organization,
            _ => ProfileKind.User
        };
    }

    /// <summary>
    ///     Converts <see cref="RequestedProfileKind" /> to its <see cref="ProfileKind" /> representation.
    /// </summary>
    /// <param name="profileKind">The <see cref="RequestedProfileKind" /> to be converted.</param>
    /// <returns>The suitable <see cref="ProfileKind" /> of <paramref name="profileKind" />.</returns>
    public static RequestedProfileKind Convert(this ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => RequestedProfileKind.Group,
            ProfileKind.User => RequestedProfileKind.User,
            ProfileKind.Organization => RequestedProfileKind.Organization,
            _ => RequestedProfileKind.All
        };
    }

    /// <summary>
    ///     Converts <see cref="ProfileContainerType" /> to its <see cref="RequestedProfileKind" /> representation.
    /// </summary>
    /// <param name="containerType">The <see cref="ProfileContainerType" /> to be converted.</param>
    /// <returns>The suitable <see cref="RequestedProfileKind" /> of <paramref name="containerType" />.</returns>
    public static RequestedProfileKind Convert(this ProfileContainerType containerType)
    {
        return containerType switch
        {
            ProfileContainerType.Group => RequestedProfileKind.Group,
            ProfileContainerType.Organization => RequestedProfileKind.Organization,
            _ => RequestedProfileKind.All
        };
    }

    /// <summary>
    ///     Converts a <see cref="ContainerType" /> to its <see cref="ObjectType" /> representation.
    /// </summary>
    /// <param name="containerType">The container type to be converted.</param>
    /// <returns>The resulting object type</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="containerType" /> is out of specified range the enum.</exception>
    public static ObjectType ToObjectType(this ContainerType containerType)
    {
        return containerType switch
        {
            ContainerType.Function => ObjectType.Function,
            ContainerType.Group => ObjectType.Group,
            ContainerType.Organization => ObjectType.Organization,
            ContainerType.Role => ObjectType.Role,
            ContainerType.NotSpecified => ObjectType.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(containerType), containerType, null)
        };
    }
}
