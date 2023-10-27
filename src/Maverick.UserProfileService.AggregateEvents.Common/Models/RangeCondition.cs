using System;

namespace Maverick.UserProfileService.AggregateEvents.Common.Models
{
    /// <summary>
    ///     Defines the date time condition for object assignments.
    /// </summary>
    public class RangeCondition
    {
        /// <summary>
        ///     Time from which the assignment has expired.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        ///     Time from which the assignment is valid.
        /// </summary>
        public DateTime? Start { get; set; }
    }
}
