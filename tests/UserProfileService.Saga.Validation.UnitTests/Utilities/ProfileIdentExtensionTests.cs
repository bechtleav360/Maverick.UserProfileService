using System;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Saga.Validation.Utilities;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ProfileIdentExtensionTests
    {
        [Theory]
        [InlineData(ProfileKind.Group, ObjectType.Group)]
        [InlineData(ProfileKind.User, ObjectType.User)]
        [InlineData(ProfileKind.Unknown, ObjectType.Profile)]
        [InlineData(ProfileKind.Organization, ObjectType.Organization)]
        public void ToObjectIdent_Success(ProfileKind kind, ObjectType expectedType)
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var profileIdent = new ProfileIdent(id, kind);

            // Act
            var objectIdent = profileIdent.ToObjectIdent();

            // Arrange
            Assert.NotNull(objectIdent);
            Assert.Equal(expectedType, objectIdent.Type);
            Assert.Equal(id, objectIdent.Id);
        }

        [Fact]
        public void ToObjectIdent_Should_Throw_ArgumentNullException_IfProfileIdentIsNull()
        {
            // Arrange
            ProfileIdent profileIdent = null;

            // Act & Arrange
            Assert.Throws<ArgumentNullException>(() => profileIdent.ToObjectIdent());
        }
    }
}
