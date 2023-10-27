using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Represents an "extended" version of a user based on <see cref="UserBasic" />. It contains more properties thant its
    ///     base class.
    /// </summary>
    public class UserView : UserBasic
    {
        /// <summary>
        ///     Contains a list of functions associated to the current profile.
        /// </summary>
        public IList<ILinkedObject> Functions { get; set; } = new List<ILinkedObject>();

        /// <summary>
        ///     Identifies the group that the user is part of.
        /// </summary>
        public IList<Member> MemberOf { set; get; } = new List<Member>();
    }
}
