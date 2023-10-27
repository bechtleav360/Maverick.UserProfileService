namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The pagination data of requests containing a <see cref="ViewFilterModel" />.
    /// </summary>
    public class PaginationData
    {
        /// <summary>
        ///     The maximum amount of result entries shown in a result set.
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        ///     The amount of items that will be skipped when returning a result set.
        ///     The sort order is relevant, which objects will be skipped.
        /// </summary>
        public int Offset { get; set; } = 0;
    }
}
