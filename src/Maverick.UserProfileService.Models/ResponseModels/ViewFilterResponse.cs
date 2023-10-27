using System.Collections.Generic;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     The response of the "get-filters-of-view" endpoint that will be return all possible filter options of a data store
    ///     (like users).
    /// </summary>
    public class ViewFilterResponse
    {
        /// <summary>
        ///     The incoming query filter without parsing it.
        /// </summary>
        public string RawFilter { get; set; }

        /// <summary>
        ///     The resulting filter options as list.
        /// </summary>
        public List<PaginatedResponseResult> RequestedFilters { get; set; }
    }
}
