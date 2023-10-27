using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines a model wrapping all properties required for creating function.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class FunctionCreated : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

        /// <summary>
        ///     A unique string to identify a function.
        /// </summary>
        public string Id { set; get; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The Id of the organization.
        /// </summary>
        public SensitiveReference Organization { get; set; }

        /// <summary>
        ///     A string to identify the role linked with this function.
        /// </summary>
        public Role Role { set; get; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Tags to assign to group.
        /// </summary>
        public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

        ///<inheridoc />
        public string Type => nameof(FunctionCreated);
    }
}
