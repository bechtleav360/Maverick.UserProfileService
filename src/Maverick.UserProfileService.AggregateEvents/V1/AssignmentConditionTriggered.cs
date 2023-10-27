using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when the assignment state has been changed (cause of conditional assignment).
    /// </summary>
    public class AssignmentConditionTriggered : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Defines whether the new state of the assignment is "ACTIVE" or not.
        /// </summary>
        public bool IsActive { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     Defines the id of the profile whose assignment status has been changed.
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        ///     Defines the id of the parent object of the assignment (i.e group or function).
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        ///     The type of the target object.
        /// </summary>
        public ObjectType TargetObjectType { get; set; }

        ///<inheridoc />
        public string Type => nameof(AssignmentConditionTriggered);
    }
}
