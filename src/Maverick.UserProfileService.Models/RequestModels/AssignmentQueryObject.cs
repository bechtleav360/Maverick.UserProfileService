namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Objects to setup a search request which specifies if only active assignments should be returned.
    /// </summary>
    public class AssignmentQueryObject : QueryObject
    {
        /// <summary>
        ///     Specifies whether only active assignment should be returned in the result set.
        /// </summary>
        public bool IncludeInactiveAssignments { get; set; } = true;
    }
}
