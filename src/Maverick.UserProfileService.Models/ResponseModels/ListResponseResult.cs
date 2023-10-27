using System.Collections.Generic;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     This result can be user for all get methods
    ///     that returns more than one result.
    /// </summary>
    /// <typeparam name="T">The result that can be transformed in every needed result.</typeparam>
    public class ListResponseResult<T>
    {
        /// <summary>
        ///     Contains the pagination metadata of the current result set.
        /// </summary>
        public ListResponse Response { set; get; }

        /// <summary>
        ///     A sequence of result items.
        /// </summary>
        public IEnumerable<T> Result { set; get; }
    }
}
