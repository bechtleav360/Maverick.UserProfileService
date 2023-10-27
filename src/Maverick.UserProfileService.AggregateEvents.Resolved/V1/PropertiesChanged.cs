using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     This event is emitted when properties of an object (i.e. profile, function, etc.) has been updated.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class PropertiesChanged : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The type of the object whose properties has been changed.
        /// </summary>
        public ObjectType ObjectType { get; set; }

        /// <summary>
        ///     Contains all changed properties with their new value.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        ///     The context defines which members have to be changed due to
        ///     the profile change.
        /// </summary>
        public PropertiesChangedContext RelatedContext { get; set; }

        ///<inheridoc />
        public string Type => nameof(PropertiesChanged);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type}{Environment.NewLine}"
                + $"Object id: {Id}{Environment.NewLine}"
                + $"Object type: {ObjectType:G}{Environment.NewLine}"
                + $"Event id: {EventId}{Environment.NewLine}"
                + $"Changed properties: {Properties?.Count ?? -1} elements{Environment.NewLine}"
                + MetaData;
        }
    }
}
