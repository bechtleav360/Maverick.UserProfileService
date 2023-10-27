using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when a child profile has been added to a <see cref="Group" />.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class WasAssignedToGroup : WasAssignedToBase<Group>
    {
    }
}
