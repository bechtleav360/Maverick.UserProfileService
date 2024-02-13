using System.Collections.Generic;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Queries.Language.Models;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Common.V2.Models;

/// <summary>
///     Options used for querying data stored in the volatile data store.
///     The results can be sorted using <see cref="OrderByList"/> and filtered
///     using the <see cref="FilterTree"/>.
/// </summary>
public class QueryOptionsVolatileModel : QueryOptions
{   /// <summary>
    ///     Gets or sets the filter tree.
    /// </summary>
    public TreeNode FilterTree { get; set; } = null;

    /// <summary>
    ///     Gets or sets a collectoion of <see cref="SortedProperty"/>s to sort the results by.
    /// </summary>
    public IEnumerable<SortedProperty> OrderByList { get; set; } = null;
}
