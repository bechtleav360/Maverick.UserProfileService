using System;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Abstractions.Helpers;

/// <summary>
///     Contains static methods to calculate compound keys of <see cref="FirstLevelProjectionTemporaryAssignment" />s.
/// </summary>
public static class CompoundKeyHelpers
{
    /// <summary>
    ///     Calculates the compound key of a <see cref="FirstLevelProjectionTemporaryAssignment" /> object.
    /// </summary>
    /// <param name="assignment">The temporary assignment entry whose compound key should be calculated.</param>
    /// <returns>The compound key of <paramref name="assignment" /> as string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="assignment" /> is <c>null</c></exception>
    public static string CalculateCompoundKey(this FirstLevelProjectionTemporaryAssignment assignment)
    {
        if (assignment == null)
        {
            throw new ArgumentNullException(nameof(assignment), "No temporary assignment has been provided.");
        }

        return
            CalculateCompoundKey(
                assignment.ProfileId,
                assignment.TargetId,
                assignment.Start,
                assignment.End);
    }

    /// <summary>
    ///     Calculates the compound key of specified parameters of a temporary assignment.
    /// </summary>
    /// <param name="profileId">The id of the assigned profile.</param>
    /// <param name="targetId">The id of the object the profile has been assigned to.</param>
    /// <param name="start">The time from when the assignment is active.</param>
    /// <param name="end">The time until when the assignment is active.</param>
    /// <returns>The compound key of all provided parameters as string.</returns>
    public static string CalculateCompoundKey(
        string profileId,
        string targetId,
        DateTime? start,
        DateTime? end)
    {
        string startString = start.HasValue
            ? start.Value.ToUniversalTime().ToString("yyyyMMddHHmmss")
            : string.Empty;

        string endString = end.HasValue
            ? end.Value.ToUniversalTime().ToString("yyyyMMddHHmmss")
            : string.Empty;

        return
            $"{profileId}:{targetId}:{startString}:{endString}";
    }
}
