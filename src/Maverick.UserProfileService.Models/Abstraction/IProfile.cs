using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     The base profile interface
    /// </summary>
    public interface IProfile
    {
        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        DateTime CreatedAt { set; get; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        string DisplayName { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        string Id { set; get; }

        /// <summary>
        ///     A profile kind is used to identify a profile. Either it is group or a user.
        /// </summary>
        ProfileKind Kind { set; get; }

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        string Name { set; get; }

        /// <summary>
        ///     The key of the source system the entity is synced from.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The url of the source that contains detailed information about related tags.
        /// </summary>
        string TagUrl { set; get; }

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        DateTime UpdatedAt { set; get; }
    }
}
