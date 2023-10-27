using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request that is send to the API to create a user profile.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        ///     A name for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     The email address of a users.
        /// </summary>
        public string Email { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     The first name of the user.
        /// </summary>
        public string FirstName { set; get; }

        /// <summary>
        ///     The last name of the user.
        /// </summary>
        public string LastName { set; get; }

        /// <summary>
        ///     The name of the user.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     Tags to assign to user.
        /// </summary>
        public IList<TagAssignment> Tags { set; get; } = new List<TagAssignment>();

        /// <summary>
        ///     An alternative name for the user.
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        ///     Assignment status of a users.
        /// </summary>
        public string UserStatus { set; get; }
    }
}
