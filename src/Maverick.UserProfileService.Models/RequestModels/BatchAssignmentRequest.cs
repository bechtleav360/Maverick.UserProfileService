using System;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Contains information how to modify the relationship of a profile (which assignments to be added o removed). The
    ///     modifications will only relate to the relationship between objects. It won't change the object itself (like
    ///     deleting them).
    /// </summary>
    public class BatchAssignmentRequest
    {
        /// <summary>
        ///     Contains assignments to be added.
        /// </summary>
        public ConditionAssignment[] Added { get; set; } = Array.Empty<ConditionAssignment>();

        /// <summary>
        ///     Contains assignments to be removed.
        /// </summary>
        public ConditionAssignment[] Removed { get; set; } = Array.Empty<ConditionAssignment>();
    }
}
