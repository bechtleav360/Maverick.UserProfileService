using System.Collections.Generic;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Queries.Language.Models;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Common.V2.Models;

public class QueryOptionsVolatileModel : QueryOptions
{
    public TreeNode FilterTree { get; set; } = null;

    public IEnumerable<SortedProperty> OrderByList { get; set; } = null;
}
