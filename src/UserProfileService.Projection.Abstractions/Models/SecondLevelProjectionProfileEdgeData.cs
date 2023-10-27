using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Contains information that have to be save in each edges inside the path calculation graph.
/// </summary>
public class SecondLevelProjectionProfileEdgeData
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     The profileId of the edge owner.
    /// </summary>
    public string RelatedProfileId { get; set; }
}
