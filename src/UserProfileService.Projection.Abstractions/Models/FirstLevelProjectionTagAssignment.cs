namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     It is used to assign tags to entities.
/// </summary>
public class FirstLevelProjectionTagAssignment
{
    /// <summary>
    ///     A boolean value that is true if the tag should be inherited.
    /// </summary>
    public bool IsInheritable { set; get; } = false;

    /// <summary>
    ///     The id representing the unique identifier of this tag
    /// </summary>
    public string TagId { get; set; }
}
