using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Contains properties to set up filter options and pagination, sorting of returned result sets.
    /// </summary>
    /// <typeparam name="TFilter">Filter type</typeparam>
    public class QueryObjectBase<TFilter> : QueryObjectBase
    {
        /// <summary>
        ///     The object that is used for filtering.
        /// </summary>
        public virtual TFilter Filter { set; get; }
    }

    /// <summary>
    ///     Contains properties to set up filter options and pagination, sorting of returned result sets.
    /// </summary>
    public class QueryObjectBase : IQueryObject
    {
        /// <summary>
        ///     The number of items to return.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Limit { set; get; } = 100;

        /// <summary>
        ///     The number of items to skip before starting to collect the result set.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Offset { set; get; }

        /// <summary>
        ///     Defines the name of the property to be sorted by.
        /// </summary>
        public string OrderedBy { set; get; }

        /// <summary>
        ///     Defines the sort direction.
        /// </summary>
        public SortOrder SortOrder { set; get; }
    }
}
