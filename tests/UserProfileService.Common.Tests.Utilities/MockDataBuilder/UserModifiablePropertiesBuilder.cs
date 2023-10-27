using System.Linq;
using Maverick.UserProfileService.Models.Modifiable;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     A class builder for <see cref="UserModifiableProperties" /> objects.
    /// </summary>
    public class
        UserModifiablePropertiesBuilder : AbstractMockBuilder<UserModifiablePropertiesBuilder, UserModifiableProperties>
    {
        /// <summary>
        ///     Default constructor to initialize a <see cref="UserModifiableProperties" />
        /// </summary>
        public UserModifiablePropertiesBuilder()
        {
            Mockedobject = new UserModifiableProperties();
        }

        /// <inheritdoc />
        public override UserModifiablePropertiesBuilder GenerateSampleData()
        {
            Mockedobject = MockDataGenerator.GenerateUserModifiableProperties().FirstOrDefault();

            return this;
        }

        /// <summary>
        ///     Create a <see cref="UserModifiableProperties" /> with the specified username
        /// </summary>
        /// <param name="userName">specified username</param>
        /// <returns>
        ///     <see cref="UserModifiablePropertiesBuilder" />
        /// </returns>
        public UserModifiablePropertiesBuilder WithUserName(string userName)
        {
            Mockedobject.UserName = userName;

            return this;
        }

        /// <summary>
        ///     Create a <see cref="UserModifiableProperties" /> with the specified display name
        /// </summary>
        /// <param name="displayName">specified display name</param>
        /// <returns>
        ///     <see cref="UserModifiablePropertiesBuilder" />
        /// </returns>
        public UserModifiablePropertiesBuilder WithDisplayName(string displayName)
        {
            Mockedobject.DisplayName = displayName;

            return this;
        }
    }
}
