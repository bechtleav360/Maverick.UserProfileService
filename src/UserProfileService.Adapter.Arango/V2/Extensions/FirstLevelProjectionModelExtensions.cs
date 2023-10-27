using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class FirstLevelProjectionModelExtensions
{
    /// <summary>
    ///     Tries to fetch the coherent model type of the given object type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="precise">If set to true, the concise model types will be returned instead of IFirstLevelProjectionProfile.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    internal static Type ToFirstLevelEntityType(this ObjectType type, bool precise = true)
    {
        Type value = type switch
        {
            ObjectType.Profile => typeof(IFirstLevelProjectionProfile),
            ObjectType.User => precise
                ? typeof(FirstLevelProjectionUser)
                : typeof(IFirstLevelProjectionProfile),
            ObjectType.Group => precise
                ? typeof(FirstLevelProjectionGroup)
                : typeof(IFirstLevelProjectionProfile),
            ObjectType.Organization => precise
                ? typeof(FirstLevelProjectionOrganization)
                : typeof(IFirstLevelProjectionProfile),
            ObjectType.Role => typeof(FirstLevelProjectionRole),
            ObjectType.Function => typeof(FirstLevelProjectionFunction),
            ObjectType.Tag => typeof(FirstLevelProjectionTag),
            _ => throw new NotSupportedException($"The object type {type} is not supported.")
        };

        return value;
    }

    internal static Type ToFirstLevelEntityType(this ContainerType type)
    {
        Type value = type switch
        {
            ContainerType.Group => typeof(FirstLevelProjectionGroup),
            ContainerType.Organization => typeof(FirstLevelProjectionOrganization),
            ContainerType.Role => typeof(FirstLevelProjectionRole),
            ContainerType.Function => typeof(FirstLevelProjectionFunction),
            _ => throw new NotSupportedException($"The object type {type} is not supported.")
        };

        return value;
    }
}
