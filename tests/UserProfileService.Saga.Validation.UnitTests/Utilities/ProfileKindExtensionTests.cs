using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Saga.Validation.Utilities;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ProfileKindExtensionTests
    {
        [Theory]
        [InlineData(ProfileKind.Group, ObjectType.Group)]
        [InlineData(ProfileKind.User, ObjectType.User)]
        [InlineData(ProfileKind.Unknown, ObjectType.Profile)]
        [InlineData(ProfileKind.Organization, ObjectType.Organization)]
        public void ToObjectType_Success(ProfileKind kind, ObjectType expectedType)
        {
            // Act
            var actualType = kind.ToObjectType();

            // Assert
            Assert.Equal(expectedType, actualType);
        }
    }
}
