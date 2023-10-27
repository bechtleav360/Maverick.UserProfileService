namespace Maverick.UserProfileService.AggregateEvents.Common.Models
{
    /// <summary>
    ///     It is used to assign tags to entities.
    /// </summary>
    public class TagAssignment
    {
        /// <summary>
        ///     A boolean value that is true if the tag should be inherited.
        ///     For entities like functions, roles and users it will be
        ///     ignored.
        /// </summary>
        public bool IsInheritable { set; get; } = false;

        /// <summary>
        ///     The id representing the unique identifier of this tag
        /// </summary>
        public Tag TagDetails { get; set; }
    }
}
