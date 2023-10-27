using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Query object used in list methods.
    /// </summary>
    public class QueryObjectList : QueryObjectBase<string>
    {
        /// <inheritdoc cref="QueryObjectBase{TFilter}" />
        [MinLength(1, ErrorMessage = "Minimum length should be 1.")]
        [NotEmptyOrWhitespace]
        public override string Filter { get; set; }

        /// <summary>
        ///     The type of profile (group, user, both = user + group).
        /// </summary>
        public RequestedProfileKind ProfileKind { get; set; }

        /// <summary>
        ///     Tags that can be applied to specified groups
        /// </summary>
        public IList<string> Tags { get; set; }
    }
}
