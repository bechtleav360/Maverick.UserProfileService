using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Helpers;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     Contains extension methods for <see cref="RangeCondition" /> related to first-level-projections.
/// </summary>
public static class RangeConditionExtensions
{
    /// <summary>
    ///     Calculates the compound key to be used with <see cref="FirstLevelProjectionTemporaryAssignment" />.
    /// </summary>
    /// <param name="conditionSet">A set of condition when the assignment is active.</param>
    /// <param name="profileId">The id of the profile that is assigned to target.</param>
    /// <param name="targetId">The id of the object the profile is assigned to.</param>
    /// <returns>A sequence of possible compound keys.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="conditionSet" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="profileId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="targetId" /> is <c>null</c>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="profileId" /> is empty or whitespace<br />-or-<br />
    ///     <paramref name="targetId" /> is empty or whitespace
    /// </exception>
    public static IEnumerable<string> CalculateTemporaryAssignmentCompoundKeys(
        this IEnumerable<RangeCondition> conditionSet,
        string profileId,
        string targetId)
    {
        if (conditionSet == null)
        {
            throw new ArgumentNullException(nameof(conditionSet));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (targetId == null)
        {
            throw new ArgumentNullException(nameof(targetId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("profileId cannot be empty or whitespace.", nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("targetId cannot be empty or whitespace.", nameof(targetId));
        }

        return conditionSet
            .Select(c => c.CalculateTemporaryAssignmentCompoundKey(profileId, targetId));
    }

    /// <summary>
    ///     Calculates the compound key to be used with <see cref="FirstLevelProjectionTemporaryAssignment" />.
    /// </summary>
    /// <param name="conditionEntry">The condition when the assignment is active.</param>
    /// <param name="profileId">The id of the profile that is assigned to target.</param>
    /// <param name="targetId">The id of the object the profile is assigned to.</param>
    /// <returns>A sequence of possible compound keys.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="conditionEntry" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="profileId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="targetId" /> is <c>null</c>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="profileId" /> is empty or whitespace<br />-or-<br />
    ///     <paramref name="targetId" /> is empty or whitespace
    /// </exception>
    public static string CalculateTemporaryAssignmentCompoundKey(
        this RangeCondition conditionEntry,
        string profileId,
        string targetId)
    {
        if (conditionEntry == null)
        {
            throw new ArgumentNullException(nameof(conditionEntry));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (targetId == null)
        {
            throw new ArgumentNullException(nameof(targetId));
        }

        return CompoundKeyHelpers.CalculateCompoundKey(
            profileId,
            targetId,
            conditionEntry.Start,
            conditionEntry.End);
    }
}
