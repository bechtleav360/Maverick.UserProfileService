using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The result for the method
///     <see cref="IFirstLevelProjectionRepository.GetDifferenceInParentsTreesAsync" />.
///     Return all parent relations of the profile identified by <see cref="Profile" />
///     that are <b>not</b> part of parent relations identified by <see cref="ReferenceProfileId" />.
/// </summary>
public class FirstLevelProjectionParentsTreeDifferenceResult
{
    /// <summary>
    ///     The tree edge relation contains all relevant information for a relation within a tree.
    ///     It stores the missing relations of the <see cref="Profile" /> with respect <see cref="ReferenceProfileId" />.
    /// </summary>
    public IList<FirstLevelProjectionTreeEdgeRelation> MissingRelations { get; set; }

    /// <summary>
    ///     The profile that missing relations should be returned in reference to <see cref="ReferenceProfileId" />.
    /// </summary>
    public IFirstLevelProjectionContainer Profile { get; set; }

    /// <summary>
    ///     The tags assignment that are added new to the tree.
    /// </summary>
    public IList<TagAssignment> ProfileTags { get; set; }

    /// <summary>
    ///     The <see cref="ReferenceProfileId" /> is used to compare with the <see cref="Profile" /> to get the difference.
    /// </summary>
    public string ReferenceProfileId { set; get; }
}
