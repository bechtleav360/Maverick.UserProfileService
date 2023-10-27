using System;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Assignment of profile with condition.
    /// </summary>
    public class ConditionAssignment
    {
        /// <summary>
        ///     Condition when the assignment is valid.
        /// </summary>
        public RangeCondition[] Conditions { get; set; } = Array.Empty<RangeCondition>();

        /// <summary>
        ///     Id of object to assign.
        /// </summary>
        public string Id { get; set; }
    }
}
