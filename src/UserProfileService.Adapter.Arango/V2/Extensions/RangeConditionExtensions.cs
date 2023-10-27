using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions.Models;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class RangeConditionExtensions
{
    private static TemporaryAssignmentState ToTemporaryAssignmentState(
        this RangeCondition condition,
        DateTime referenceTime)
    {
        if (condition.Start == null
            || condition.Start.Value.ToUniversalTime() < referenceTime)
        {
            return condition.End == null || condition.End == DateTime.MaxValue.ToUniversalTime()
                ? TemporaryAssignmentState.Active
                : TemporaryAssignmentState.ActiveWithExpiration;
        }

        return TemporaryAssignmentState.NotProcessed;
    }

    /// <summary>
    ///     Will convert the range condition if it should be treated as temporary to
    ///     <see cref="FirstLevelProjectionTemporaryAssignment" />s.<br />
    ///     BEWARE: Profile type will be missing, and must be added afterwards.<br />
    ///     If the condition is not temporary, the method will return <c>null</c>.
    /// </summary>
    internal static FirstLevelProjectionTemporaryAssignment ToTemporaryAssignment(
        this RangeCondition condition,
        string childId,
        string parentId,
        ContainerType parentType)
    {
        DateTime current = DateTime.UtcNow;

        if (condition == null
            || ((condition.Start == null
                    || condition.Start.Value.ToUniversalTime() < current)
                && (condition.End == null
                    || condition.End == DateTime.MaxValue))
            || (condition.End != null
                && condition.End.Value.ToUniversalTime() < current))
        {
            return default;
        }

        return new FirstLevelProjectionTemporaryAssignment
        {
            Id = Guid.NewGuid().ToString(),
            Start = condition.Start,
            End = condition.End,
            LastModified = DateTime.UtcNow,
            ProfileId = childId,
            TargetId = parentId,
            TargetType = parentType.ToObjectType(),
            ProfileType = ObjectType.Unknown,
            State = condition.ToTemporaryAssignmentState(current)
        };
    }

    /// <summary>
    ///     Will convert all range conditions that will be temporary to <see cref="FirstLevelProjectionTemporaryAssignment" />
    ///     s.<br />
    ///     BEWARE: Profile type will be missing, and must be added afterwards.
    /// </summary>
    internal static IList<FirstLevelProjectionTemporaryAssignment> ToTemporaryAssignments(
        this IEnumerable<RangeCondition> conditions,
        string childId,
        string parentId,
        ContainerType parentType)
    {
        return conditions?
                .Select(c => c.ToTemporaryAssignment(childId, parentId, parentType))
                .Where(ta => ta != null)
                .ToList()
            ?? new List<FirstLevelProjectionTemporaryAssignment>();
    }
}
