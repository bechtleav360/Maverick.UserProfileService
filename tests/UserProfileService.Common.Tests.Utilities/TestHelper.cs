using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Xunit;

namespace UserProfileService.Common.Tests.Utilities
{
    /// <summary>
    ///     Class used to compare some model objects each other
    /// </summary>
    public class TestHelper
    {
        /// <summary>
        ///     Compare 2 <see cref="IProfile" /> each other
        /// </summary>
        public static void Equal(IProfile expectedUser, IProfile actualUser)
        {
            Assert.NotNull(expectedUser);
            Assert.NotNull(actualUser);
            Assert.Equal(expectedUser.CreatedAt, actualUser.CreatedAt);
            Assert.Equal(expectedUser.Name, actualUser.Name);
            Assert.Equal(expectedUser.DisplayName, actualUser.DisplayName);
            Assert.Equal(expectedUser.ExternalIds, actualUser.ExternalIds);
            Assert.Equal(expectedUser.Id, actualUser.Id);
            Assert.Equal(expectedUser.Kind, actualUser.Kind);
            Assert.Equal(expectedUser.TagUrl, actualUser.TagUrl);
            Assert.Equal(expectedUser.UpdatedAt, actualUser.UpdatedAt);
        }

        /// <summary>
        ///     Compare 2 <see cref="RoleBasic" /> each other
        /// </summary>
        public static void Equal(RoleBasic expectedRole, RoleBasic actualRole)
        {
            Assert.NotNull(expectedRole);
            Assert.NotNull(actualRole);
            Assert.Equal(expectedRole.Id, actualRole.Id);
            Assert.Equal(expectedRole.Type, actualRole.Type);
            Assert.Equal(expectedRole.Description, actualRole.Description);
            Assert.Equal(expectedRole.IsSystem, actualRole.IsSystem);
            Assert.Equal(expectedRole.Permissions, actualRole.Permissions);
        }

        /// <summary>
        ///     Compare 2 collections of <see cref="RoleBasic" /> each other
        /// </summary>
        public static void Equal(IEnumerable<RoleBasic> expectedRoles, IEnumerable<RoleBasic> actualRoles)
        {
            Assert.NotNull(expectedRoles);
            Assert.NotNull(actualRoles);
            Assert.Equal(expectedRoles.Count(), actualRoles.Count());

            for (var i = 0; i < expectedRoles.Count(); i++)
            {
                Equal(expectedRoles.ElementAtOrDefault(i), actualRoles.ElementAtOrDefault(i));
            }
        }

        /// <summary>
        ///     Compare 2 collections of <see cref="IProfile" /> each other
        /// </summary>
        public static void Equal(IEnumerable<IProfile> expectedUsers, IEnumerable<IProfile> actualUsers)
        {
            Assert.NotNull(expectedUsers);
            Assert.NotNull(actualUsers);
            Assert.Equal(expectedUsers.Count(), actualUsers.Count());

            for (var i = 0; i < expectedUsers.Count(); i++)
            {
                Equal(expectedUsers.ElementAtOrDefault(i), actualUsers.ElementAtOrDefault(i));
            }
        }
    }
}
