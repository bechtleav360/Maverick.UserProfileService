using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="AssignmentType" />s.
/// </summary>
public static class AssignmentTypeExtension
{
    /// <summary>
    ///     Check if two relation types exist as an opposite relation.
    ///     Father -> children (children), children -> father (parent)
    /// </summary>
    /// <param name="sourceType">Source type to check.</param>
    /// <param name="targetType">targetType to check.</param>
    /// <returns>True if exists, otherwise false.</returns>
    public static bool CompareOppositeRelationType(this AssignmentType sourceType, AssignmentType targetType)
    {
        return sourceType switch
        {
            AssignmentType.ChildrenToParent => targetType == AssignmentType.ParentsToChild,
            AssignmentType.ParentsToChild => targetType == AssignmentType.ChildrenToParent,
            _ => sourceType == targetType
        };
    }
}
