using System;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Helpers;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Represents stored temporary assignments (active and inactive).
/// </summary>
public class FirstLevelProjectionTemporaryAssignment
{
    /// <summary>
    ///     A combination of the main identifying properties: <see cref="ProfileId" />, <see cref="TargetId" />,
    ///     <see cref="Start" /> and <see cref="End" />.
    /// </summary>
    public string CompoundKey => this.CalculateCompoundKey();

    /// <summary>
    ///     Time from which the assignment has expired.
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    ///     The id of the entry.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The last occurred error.
    /// </summary>
    public string LastErrorMessage { get; set; }

    /// <summary>
    ///     The time when the current item has been modified lastly.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    ///     The notification status of the temporary assignment. If the assignment
    ///     has a clear start  and end date (assignment with expiration and the assignments begins, when the the assignments
    ///     are processed), than two notifications have to be send.
    ///     First notification is when the assignments starts and the second one, when
    ///     the assignment ends. At least one notifications has to be send.
    /// </summary>
    public NotificationStatus NotificationStatus { set; get; }

    /// <summary>
    ///     The profile to assign the target to.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     The type of the profile to be assigned to.
    /// </summary>
    public ObjectType ProfileType { get; set; }

    /// <summary>
    ///     Time from which the assignment is valid.
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    ///     The current state of the entry.
    /// </summary>
    public TemporaryAssignmentState State { get; set; }

    /// <summary>
    ///     The id of the target to assign.
    /// </summary>
    public string TargetId { get; set; }

    /// <summary>
    ///     The type of the target.
    /// </summary>
    public ObjectType TargetType { get; set; }
}
