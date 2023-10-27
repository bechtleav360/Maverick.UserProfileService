namespace Maverick.UserProfileService.Models.Modifiable
{
    /// <summary>
    ///     Contains all properties of a user that can be modified.
    /// </summary>
    public class UserModifiableProperties
    {
        /// <summary>
        ///     A name for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     The email address of a user.
        /// </summary>
        public string Email { set; get; }

        /// <summary>
        ///     The first name of the user.
        /// </summary>
        public string FirstName { set; get; }

        /// <summary>
        ///     The last name of the user.
        /// </summary>
        public string LastName { set; get; }

        /// <summary>
        ///     An alternative name for the user.
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        ///     Assignment status of a users.
        /// </summary>
        public string UserStatus { set; get; }
    }
}
