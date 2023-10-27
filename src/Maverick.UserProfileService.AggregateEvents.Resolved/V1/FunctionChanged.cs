using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when the function has been changed.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class FunctionChanged : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The context defines which members have to be changed due to
        ///     the function change.
        /// </summary>
        public PropertiesChangedContext Context { set; get; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The whole function that has been changed.
        /// </summary>
        public Function Function { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; }

        ///<inheridoc />
        public string Type => nameof(FunctionChanged);
    }
}
