using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     Response model of the GET projectionState/service endpoint.<br />
    ///     It represents a view of the whole state grouped by stream names.
    /// </summary>
    public class GroupedProjectionState : Dictionary<string, IList<ProjectionState>>
    {
        /// <summary>
        ///     Is the amount of found items regardless of the pagination settings.
        /// </summary>
        public long TotalCount { get; set; }
    }
}
