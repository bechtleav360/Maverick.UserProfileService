using System;
using FluentAssertions;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Validation.Abstractions;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ValidationResultExtensionTests
    {
        [Fact]
        public void CheckAndThrowException_Success()
        {
            // Arrange
            var validationResult = new ValidationResult();

            // Act & Assert
            validationResult.CheckAndThrowException();
        }

        [Fact]
        public void CheckAndThrowException_Should_ThrowValidationException_IfValidationResultsInvalid()
        {
            // Arrange
            var validationResult =
                new ValidationResult(
                    new ValidationAttribute("test", "test"),
                    new ValidationAttribute("test 2", "test 2"));

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => validationResult.CheckAndThrowException());

            exception.ValidationResults.Should().BeEquivalentTo(validationResult.Errors);
        }

        [Fact]
        public void CheckAndThrowException_Should_ThrowValidationException_IfValidationResultIsNull()
        {
            // Arrange
            ValidationResult validationResult = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => validationResult.CheckAndThrowException());
        }
    }
}
