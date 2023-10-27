using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     Defines a base model of a user profile.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class User
    {
        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     The domain of the user.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        ///     The email addresses of the user.
        /// </summary>
        public string Email { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

        /// <summary>
        ///     The first name of the user.
        /// </summary>
        public string FirstName { set; get; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     A profile kind is used to identify a profile. Either it is group or a user.
        /// </summary>
        public ProfileKind Kind => ProfileKind.User;

        /// <summary>
        ///     The last name of the user.
        /// </summary>
        public string LastName { set; get; }

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        ///     The name of the user.
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        ///     The image url of the group.
        /// </summary>
        public string UserStatus { set; get; }
    }
}
