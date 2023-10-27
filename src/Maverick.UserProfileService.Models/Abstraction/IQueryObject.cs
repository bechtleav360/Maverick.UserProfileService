using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Contains properties to set up pagination and sorting of query results.
    /// </summary>
    public interface IQueryObject : IPaginationSettings
    {
        /// <summary>
        ///     Defines the name of the property to be sorted by.
        /// </summary>
        string OrderedBy { set; get; }

        /// <summary>
        ///     Defines the sort direction.
        /// </summary>
        SortOrder SortOrder { set; get; }
    }
}
