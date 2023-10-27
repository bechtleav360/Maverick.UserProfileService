namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     The amount metadata of a response of the view filter endpoint.
    /// </summary>
    public class PaginatedResponse
    {
        /// <summary>
        ///     The amount of entries in the current result set.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     The total amount of entries available with the current filter options without pagination settings.
        /// </summary>
        public long TotalAmount { get; set; }
    }
}
