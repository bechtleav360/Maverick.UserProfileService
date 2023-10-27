using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Represents a single assignment between a profile and a container.
/// </summary>
public class SecondLevelProjectionAssignment
{
    /// <summary>
    ///     A list of all conditions which are linked to the assignment.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     The container to which the profile was assigned to.
    /// </summary>
    public ObjectIdent Parent { get; set; }

    /// <summary>
    ///     The profile which was assigned to the container.
    /// </summary>
    public ObjectIdent Profile { get; set; }
}
