using System;
using System.Collections.Generic;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

internal static class TemporaryAssignmentExtensions
{
    /// <summary>
    ///     Updates the state of each temporary assignment entries depending on their start and end dates compared to
    ///     the current time and returns the original collection.
    /// </summary>
    internal static IList<FirstLevelProjectionTemporaryAssignment> UpdateState(
        this IList<FirstLevelProjectionTemporaryAssignment> entries)
    {
        foreach (FirstLevelProjectionTemporaryAssignment entry in entries)
        {
            entry.UpdateState();
        }

        return entries;
    }

    /// <summary>
    ///     Update the state of a temporary assignment entry depending on its start and end date compared to the current
    ///     time.
    /// </summary>
    internal static void UpdateState(
        this FirstLevelProjectionTemporaryAssignment entry)
    {
        entry.State = entry.GetState();
    }

    // Status:
    // (we expect start < end, because of the validation during processing requests to make thins easier)
    // [also seen in TemporaryAssignmentState enum comments]
    //
    //   active: start less equal than now() AND end equals max() or null
    //   inactive: end less than now()
    //   notProcessed: start greater than now()
    //   activeWithExpiration. start less equal than now() AND end greater now() BUT less than max()
    //
    //    [---inactive---]
    //                            [-------------active--------------]
    //            [---inactive----]
    //                          [---active w/ expiration---]
    //                                       [--not processed--]
    //                          
    // MIN/NULL <------ PAST ------>| NOW |<----- FUTURE --------> MAX/NULL
    /// <summary>
    ///     Gets the current <see cref="TemporaryAssignmentState" /> of a temporary assignment <paramref name="entry" />.
    /// </summary>
    /// <remarks>
    ///     The current date (or now()) will be the reference date.<br />
    ///     <list type="number">
    ///         <item>
    ///             <term>active</term>
    ///             <description>start less equal than now() AND end equals max() or null</description>
    ///         </item>
    ///         <item>
    ///             <term>inactive</term>
    ///             <description>end less than now()</description>
    ///         </item>
    ///         <item>
    ///             <term>notProcessed</term>
    ///             <description>start greater than now()</description>
    ///         </item>
    ///         <item>
    ///             <term>activeWithExpiration</term>
    ///             <description>start less equal than now() AND end greater now() BUT less than max()</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="entry"></param>
    /// <returns></returns>
    internal static TemporaryAssignmentState GetState(this FirstLevelProjectionTemporaryAssignment entry)
    {
        DateTime now = DateTime.UtcNow;

        // the condition has not met yet -> that means the assignment has not been not processed yet
        if (entry.Start != null && entry.Start > now)
        {
            return TemporaryAssignmentState.NotProcessed;
        }

        // at this point we consider the start time either less than now() or not given (= null)
        // that means the assignment is already active

        // if end date is not provided or equals the maximum value,
        // the condition won't be deactivated in the future and therefor active forever
        // in this case the status is 'active'
        if (entry.End == null || entry.End == DateTime.MaxValue.ToUniversalTime())
        {
            return TemporaryAssignmentState.Active;
        }

        // the assignment won't be active any more
        if (entry.End < now)
        {
            return TemporaryAssignmentState.Inactive;
        }

        // the assignment won't be active forever, but 
        // therefore the status is 'activeWithExpiration'
        return TemporaryAssignmentState.ActiveWithExpiration;
    }
}
