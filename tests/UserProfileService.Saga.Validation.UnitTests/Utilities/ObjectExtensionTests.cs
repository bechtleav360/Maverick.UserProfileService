using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using UserProfileService.Saga.Validation.Utilities;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ObjectExtensionTests
    {
        [Fact]
        public void TryConvertObject_Should_Throw_ArgumentNullException_IfObjectIsNull()
        {
            // Arrange
            object obj = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => obj.TryConvertObject<TestModel>());
        }

        [Fact]
        public void TryConvertObject_Success_IfObjectIsJToken()
        {
            // Arrange
            var testModel = new TestModel();
            JToken obj = JToken.FromObject(testModel);

            // Act & Assert
            var result = obj.TryConvertObject<TestModel>();

            Assert.NotNull(result);
            testModel.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void TryConvertObject_Success_IfObjectIsJArray()
        {
            // Arrange
            var testModels = new List<TestModel>
            {
                new TestModel()
            };

            JArray obj = JArray.FromObject(testModels);

            // Act & Assert
            var result = obj.TryConvertObject<IList<TestModel>>();

            Assert.NotNull(result);
            testModels.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void TryConvertObject_Success_IfObjectImplementIConvertible()
        {
            // Arrange
            var obj = "500";

            // Act & Assert
            var result = obj.TryConvertObject<int>();

            Assert.Equal(500, result);
        }

        [Fact]
        public void TryConvertObject_Success_IfObjectTypeIsSame()
        {
            // Arrange
            var obj = "500";

            // Act & Assert
            var result = obj.TryConvertObject<string>();

            Assert.Equal("500", result);
        }

        [Fact]
        public void TryConvertObject_Should_Return_Default_IfConvertingThrowsException()
        {
            // Arrange
            var obj = "test";

            // Act & Assert
            var result = obj.TryConvertObject<int>();

            Assert.Equal(default, result);
        }
    }

    internal class TestModel
    {
        public string Id { get; set; }

        public TestModel()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
