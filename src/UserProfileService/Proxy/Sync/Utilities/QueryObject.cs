using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Proxy.Sync.Utilities;

/// <summary>
///     Contains the properties to set the sort direction, the property name to sort by and the pagination.
/// </summary>
public class QueryObject
{
    /// <summary>
    ///     Defines the name of the property to be sorted by.
    /// </summary>
    public string OrderedBy { set; get; }

    /// <summary>
    ///     Number of page to retrieve (default: 1).
    /// </summary>
    public int Page { set; get; } = 1;

    /// <summary>
    ///     Size of page to retrieve (default: 10).
    /// </summary>
    public int PageSize { set; get; } = 10;

    /// <summary>
    ///     Defines the sort direction.
    /// </summary>
    public SortOrder SortOrder { set; get; }
}
