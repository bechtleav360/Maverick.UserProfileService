using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Bundles information about a user in order to calculate assignments.
/// </summary>
public class SecondLevelProjectionAssignmentsUser
{
    /// <summary>
    ///     Contains the object ident of all containers in to which the user is currently assigned.
    ///     This value is meant to be used for queries, the other values contain raw values which are not filtered.
    /// </summary>
    public IList<ObjectIdent> ActiveMemberships { get; set; } = new List<ObjectIdent>();

    /// <summary>
    ///     A list containing all assignments.
    /// </summary>
    public IList<SecondLevelProjectionAssignment> Assignments { get; set; } =
        new List<SecondLevelProjectionAssignment>();

    /// <summary>
    ///     A list containing all containers.
    /// </summary>
    public IList<ISecondLevelAssignmentContainer> Containers { get; set; } =
        new List<ISecondLevelAssignmentContainer>();

    /// <summary>
    ///     The id of the user.
    /// </summary>
    public string ProfileId { get; set; }
}
