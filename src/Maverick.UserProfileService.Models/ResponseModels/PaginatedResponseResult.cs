using System.Collections;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     Defines a resulting entry of <see cref="ViewFilterResponse" />. It contains possible filter options.
    /// </summary>
    public class PaginatedResponseResult
    {
        /// <summary>
        ///     Information about the paginated list - like amount of resulting entries.
        /// </summary>
        public PaginatedResponse Response { set; get; }

        /// <summary>
        ///     A sequence of possible filter options of the current filter model.
        /// </summary>
        public IEnumerable Result { set; get; }

        /// <summary>
        ///     The parsed result of the provided query string.
        /// </summary>
        public ViewFilterModel ViewFilter { get; set; }
    }
}
