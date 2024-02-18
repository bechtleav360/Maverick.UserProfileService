using System;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Extensions
{
    /// <summary>
    ///     Class containing all step class extensions.
    /// </summary>
    internal static class StepExtensions
    {
        /// <summary>
        ///     Returns a boolean value whether the step is older than the specified time in minutes.
        /// </summary>
        /// <param name="step">An instance of <see cref="Step"/></param>
        /// <param name="timeInMinutes">The time to check for last update</param>
        /// <returns> True if the last update of the step is older than than the provided value, otherwise false</returns>
        public static bool StepLastUpdateIsOlderThan(this Step step, int timeInMinutes)
        {
            return DateTime.Compare(
                       DateTime.UtcNow,
                       step.UpdatedAt.GetValueOrDefault().AddMinutes(timeInMinutes))
                   > 0;
        }
    }
}
