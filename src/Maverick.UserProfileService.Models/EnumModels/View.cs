namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     The view that can be chosen.
    /// </summary>
    public enum View
    {
        /// <summary>
        ///     No view type has been requested.
        /// </summary>
        None,

        /// <summary>
        ///     The view of the users store.
        /// </summary>
        Users,

        /// <summary>
        ///     The view of the groups store.
        /// </summary>
        Groups,

        /// <summary>
        ///     The view of the functions store.
        /// </summary>
        Functions,

        /// <summary>
        ///     The view of the objects store.
        /// </summary>
        Objects,

        /// <summary>
        ///     The view of the roles store.
        /// </summary>
        Roles,

        /// <summary>
        ///     The view of the organizations store.
        /// </summary>
        Organizations
    }
}
