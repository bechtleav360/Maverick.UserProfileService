namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     This enum is used to identify what kind of member should be
    ///     updated when a property changed event occurred that affects
    ///     the members.
    /// </summary>
    public enum PropertiesChangedContext
    {
        /// <summary>
        ///     Only the properties from the own object should be updated.
        /// </summary>
        Self,

        /// <summary>
        ///     The role of the function should be updated.
        /// </summary>
        Role,

        /// <summary>
        ///     The organization of the function should be updated.
        /// </summary>
        Organization,

        /// <summary>
        ///     The security assignments should be updated.
        /// </summary>
        SecurityAssignments,

        /// <summary>
        ///     The members should be updated.
        /// </summary>
        Members,

        /// <summary>
        ///     The membersOf should be updated.
        /// </summary>
        MemberOf,

        /// <summary>
        ///     The linked profiles should be updated.
        /// </summary>
        LinkedProfiles,

        /// <summary>
        ///     The object should not be updated but is an indirect member.
        /// </summary>
        IndirectMember
    }
}
