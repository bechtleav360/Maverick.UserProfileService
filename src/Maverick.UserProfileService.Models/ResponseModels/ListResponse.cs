namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     An object for returning the results with pagination.
    /// </summary>
    public class ListResponse
    {
        /// <summary>
        ///     An object for returning the results with pagination.
        /// </summary>
        public long Count { set; get; } = 0;

        /// <summary>
        ///     Represents the link to the next page of the current request.
        ///     If no previous page can be shown, it will be null.
        /// </summary>
        public string NextLink { set; get; }

        /// <summary>
        ///     Total amount of results found.
        /// </summary>
        public string PreviousLink { set; get; }
    }
}
