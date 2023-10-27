using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Bundles information about a user in order to calculate assignments.
/// </summary>
public class SecondLevelProjectionOpaUser
{
    /// <summary>
    ///     Contains the object ident of all containers in to which the user is currently assigned.
    ///     This value is meant to be used for queries, the other values contain raw values which are not filtered.
    /// </summary>
    public IList<ObjectIdent> ActiveAssociatedProfiles { get; set; } = new List<ObjectIdent>();

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
    ///     The time at which this object was created and saved in the repository for the first time in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     The id of the user.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     The time at which this object was last saved in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
