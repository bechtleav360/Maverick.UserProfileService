using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     This event is emitted when properties of an object (i.e. profile, function, etc.) has been updated.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class PropertiesChanged : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The names of all changed properties whose values can be retrieved from sensitive data store.
        /// </summary>
        public IList<string> ChangedPropertyNames { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The reference of the modified object/entity data in the sensitive data store.
        /// </summary>
        public SensitiveReference Object { get; set; }

        /// <summary>
        ///     The type of the object whose properties has been changed.
        /// </summary>
        public ObjectType ObjectType { get; set; }

        /// <summary>
        ///     Contains all changed properties whose values are provided directly in this event, because they are not sensitive.
        ///     The keys of the dictionary are property names, the value collection contains data itself.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }

        ///<inheridoc />
        public string Type => nameof(PropertiesChanged);
    }
}
