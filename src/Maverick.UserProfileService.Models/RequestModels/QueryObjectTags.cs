using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Objects to set up a search request (without tag filters).
    /// </summary>
    public class QueryObjectTags : QueryObjectBase<Filter>
    {
        /// <summary>
        ///     Gets or sets the filter setting that can contain several options to filter a result set.
        /// </summary>
        [FilterObjectValid]
        public override Filter Filter { get; set; }

        /// <summary>
        ///     The search text to apply to the result.
        /// </summary>
        [NotEmptyOrWhitespace(
            ErrorMessage =
                "Value of {0} must be a valid string (not null, not an empty string, not only whitespaces).")]
        public string Search { set; get; }
    }
}
