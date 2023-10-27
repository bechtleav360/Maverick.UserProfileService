using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Contains information that have to be save in each vertex inside the path calculation graph.
/// </summary>
public class SecondLevelProjectionProfileVertexData
{
    /// <summary>
    ///     The Id of the referenced document.
    /// </summary>
    public string ObjectId { get; set; }

    /// <summary>
    ///     The profileId of the vertex owner.
    /// </summary>
    public string RelatedProfileId { get; set; }

    /// <summary>
    ///     The list of tags as <see cref="TagAssignment" /> contained in the entity that the vertex represents.
    /// </summary>
    public IList<TagAssignment> Tags { get; set; } = new List<TagAssignment>();
}
