namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Represents a view filter used in the filter-view endpoint.<br />
    ///     It is used as a result of parsing the incoming query string.
    /// </summary>
    public class ViewFilterModel
    {
        /// <summary>
        ///     Contains the requested entity type of the request.
        /// </summary>
        public ViewFilterDataStoreContext DataStoreContext { get; set; }

        /// <summary>
        ///     The name of the field that has been requested.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        ///     A collection of strings the should be used for filtering. Can be empty.
        /// </summary>
        public string[] Filter { get; set; }

        /// <summary>
        ///     Contains  pagination setting of the request.
        /// </summary>
        public PaginationData Pagination { get; set; } = new PaginationData();

        /// <summary>
        ///     The type of the view filter - this controls the kind of the resulting entries in the result set.
        /// </summary>
        public ViewFilterTypes Type { get; set; }
    }
}
