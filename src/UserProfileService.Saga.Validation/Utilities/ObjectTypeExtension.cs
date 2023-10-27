using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Contains extension methods related to <see cref="ObjectType" />s.
/// </summary>
public static class ObjectTypeExtension
{
    /// <summary>
    ///     Checks if the given object type represents a container profile.
    /// </summary>
    /// <param name="objectType">Object type to be checked.</param>
    /// <returns><see langword="true" /> if the type represents a profile; otherwise, <see langword="false" />.</returns>
    public static bool IsContainerProfileType(this ObjectType objectType)
    {
        return objectType == ObjectType.Group || objectType == ObjectType.Organization;
    }
}
