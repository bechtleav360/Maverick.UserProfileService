using Maverick.UserProfileService.Models.Abstraction;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The query option that is used to include filter options.
    /// </summary>
    public class QueryOptions : IPaginationSettings
    {
        /// <summary>
        ///     The filter query that is used to filter the result objects.
        /// </summary>
        public virtual string Filter { set; get; }

        /// <summary>
        ///     The number of items to return.
        /// </summary>
        public virtual int Limit { set; get; } = 50;

        /// <summary>
        ///     The number of items to skip before starting to collect the result set.
        /// </summary>
        public virtual int Offset { set; get; } = 1;

        /// <summary>
        ///     The oder by query that is used to order the result objects for one
        ///     or more specific Property.
        /// </summary>
        public virtual string OrderBy { set; get; }
    }
}
