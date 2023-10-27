namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Type to specify assignment between to objects.
    /// </summary>
    public enum AssignmentType
    {
        /// <summary>
        ///     The type is not known.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Objects are added to resource as children.
        /// </summary>
        ChildrenToParent = 1,

        /// <summary>
        ///     Objects are added to resource as parents.
        /// </summary>
        ParentsToChild = 2
    }
}
