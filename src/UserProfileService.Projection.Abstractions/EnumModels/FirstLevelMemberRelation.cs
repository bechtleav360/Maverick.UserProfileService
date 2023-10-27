namespace UserProfileService.Projection.Abstractions.EnumModels;

/// <summary>
///     States the relation from an entity to another.
/// </summary>
public enum FirstLevelMemberRelation
{
    /// <summary>
    ///     States that the member is a direct member.
    /// </summary>
    DirectMember,

    /// <summary>
    ///     States that the member is an indirect member.
    /// </summary>
    IndirectMember
}
