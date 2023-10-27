namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    /// <summary>
    ///     Contains default id/names of all security relevant entities that can be used in tests/fixtures.
    /// </summary>
    public static class DefaultProfileSampleConstants
    {
        /// <summary>
        ///     The id of the root group that has no other relations (no tags).
        /// </summary>
        public const string RootGroupLonely = "root-group-lonely";

        /// <summary>
        ///     The id of the user who is not member of any group.
        /// </summary>
        public const string UserLonely = "user-in-no-group-1";

        /// <summary>
        ///     The id of the user who is in the default root group.
        /// </summary>
        public const string UserInRootGroup = "user-in-root-group-1";

        /// <summary>
        ///     The id of the default group in a group (no tags).
        /// </summary>
        public const string GroupInGroup = "group-child-of-group-1";

        /// <summary>
        ///     The id of the default root group (no tags).
        /// </summary>
        public const string RootGroup = "root-group-1";

        /// <summary>
        ///     User in the group that is in the root group itself.
        /// </summary>
        public const string UserInGroupOfRootGroup = "user-in-sub-group-1";

        /// <summary>
        ///     Role of root group.
        /// </summary>
        public const string RoleOfRootGroup = "role-of-root-grp";

        /// <summary>
        ///     Role of user who is member of root group.
        /// </summary>
        public static string RoleOfUser = "role-of-user-in-root-grp";
    }
}
