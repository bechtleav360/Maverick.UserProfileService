using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Saga.Validation.Utilities;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ObjectTypeExtensionTests
    {
        [Theory]
        [InlineData(ObjectType.Organization, true)]
        [InlineData(ObjectType.User, false)]
        [InlineData(ObjectType.Function, false)]
        [InlineData(ObjectType.Group, true)]
        [InlineData(ObjectType.Profile, false)]
        [InlineData(ObjectType.Role, false)]
        [InlineData(ObjectType.Tag, false)]
        [InlineData(ObjectType.Unknown, false)]
        public void IsContainerProfileType_Success(ObjectType objectType, bool isContainerType)
        {
            // Act
            bool result = objectType.IsContainerProfileType();

            // Assert
            Assert.Equal(isContainerType, result);
        }
    }
}
