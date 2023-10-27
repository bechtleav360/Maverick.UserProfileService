using System;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Contains entity model properties of <see cref="Assignment" />s.
/// </summary>
public class AssignmentEntityModel
{
    /// <summary>
    ///     Time from which the assignment has expired.
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    ///     The profile to assign the target to.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     Time from which the assignment is valid.
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    ///     The id of the target to assign.
    /// </summary>
    public string TargetId { get; set; }

    /// <summary>
    ///     The type of the target.
    /// </summary>
    public ObjectType TargetType { get; set; }
}
