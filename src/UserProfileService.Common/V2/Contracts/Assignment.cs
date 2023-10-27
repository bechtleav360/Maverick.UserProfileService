using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Represents a single assignment.
/// </summary>
public class Assignment
{
    /// <summary>
    ///     The conditions that should apply to the assignment.
    /// </summary>
    public RangeCondition[] Conditions { get; set; }

    /// <summary>
    ///     The profile to assign the target to.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     The id of the target to assign.
    /// </summary>
    public string TargetId { get; set; }

    /// <summary>
    ///     The type of the target.
    /// </summary>
    public ObjectType TargetType { get; set; }
}
