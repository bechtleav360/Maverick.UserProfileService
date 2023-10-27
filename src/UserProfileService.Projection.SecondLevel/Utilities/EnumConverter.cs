using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.SecondLevel.Utilities;

/// <summary>
///     Contains some methods to convert enums among each other
/// </summary>
public class EnumConverter
{
    /// <summary>
    ///     Converts a <see cref="ObjectType" /> to <see cref="ContainerType" />.
    /// </summary>
    /// <param name="type"> The Object type that should be converted. </param>
    /// <returns> The converted <see cref="ContainerType" />. </returns>
    public static ContainerType ConvertObjectTypeToContainerType(ObjectType type)
    {
        return type switch
        {
            ObjectType.Group => ContainerType.Group,
            ObjectType.Role => ContainerType.Role,
            ObjectType.Function => ContainerType.Function,
            ObjectType.Organization => ContainerType.Organization,
            _ => ContainerType.NotSpecified
        };
    }
}
