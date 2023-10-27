using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Saga.Validation.Utilities;
using UserProfileService.Validation.Abstractions;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests.Utilities
{
    public class ValidatorTests
    {
        [Theory]
        [ClassData(typeof(ValidateWithRegexSuccessTestData))]
        public void ValidatorString_ValidateWithRegex_Success(
            string str,
            string pattern,
            RegexOptions regexOptions,
            bool isValid)
        {
            // Act
            ValidationResult result = Validator.String.ValidateWithRegex(str, pattern, regexOptions);

            // Assert
            Assert.Equal(isValid, result.IsValid);
        }

        [Fact]
        public void ValidatorString_ValidateWithRegex_Throw_ArgumentNullException_IfPatternIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Validator.String.ValidateWithRegex("^[a-z]+$", null));
        }

        [Theory]
        [ClassData(typeof(ValidateNamesSuccessTestData))]
        public void ValidatorGroup_ValidateNames_Success(
            string name,
            string displayName,
            string pattern,
            bool isValid,
            int results = 0)
        {
            // Act
            ValidationResult result = Validator.Group.ValidateNames(name, displayName, pattern);

            // Assert
            Assert.Equal(isValid, result.IsValid);
            Assert.Equal(results, result.Errors.Count);
        }

        [Fact]
        public void ValidatorGroup_ValidateNames_Throw_ArgumentNullException_IfPatternIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Validator.Group.ValidateNames("Group", "Group", null));
        }

        [Theory]
        [InlineData(true, InitiatorType.ServiceAccount, true)]
        [InlineData(true, InitiatorType.System, true)]
        [InlineData(true, InitiatorType.Unknown, true)]
        [InlineData(false, InitiatorType.Unknown, true)]
        [InlineData(false, InitiatorType.User, true)]
        [InlineData(false, InitiatorType.ServiceAccount, true)]
        [InlineData(false, InitiatorType.System, true)]
        [InlineData(true, null, true)]
        [InlineData(false, null, true)]
        [InlineData(true, InitiatorType.User, false)]
        public void ValidatorGroup_ValidateOperationAllowed_Success(
            bool isSystem,
            InitiatorType? initiatorType,
            bool expectedAllowed)
        {
            // Act
            ValidationResult result = Validator.Profile.ValidateOperationAllowed(isSystem, initiatorType);

            // Assert
            Assert.Equal(expectedAllowed, result.IsValid);
            Assert.Equal(expectedAllowed, result.Errors.Count == 0);
        }

        private class ValidateWithRegexSuccessTestData : IEnumerable<object[]>
        {
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { " ", "^[a-z]+$", RegexOptions.IgnoreCase, true };
                yield return new object[] { "", "^[a-z]+$", RegexOptions.IgnoreCase, true };
                yield return new object[] { "test", "^[a-z]+$", RegexOptions.IgnoreCase, true };
                yield return new object[] { "Test", "^[a-z]+$", RegexOptions.None, false };
            }
        }

        private class ValidateNamesSuccessTestData : IEnumerable<object[]>
        {
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "Group", "GroupDisplay", "^[a-z]+$", true, 0 };
                yield return new object[] { "", "GroupDisplay", "^[a-z]+$", true, 0 };
                yield return new object[] { "", "", "^[a-z]+$", true, 0 };
                yield return new object[] { " ", " ", "^[a-z]+$", true, 0 };
                yield return new object[] { "Group 1", "GroupDisplay", "^[a-z]+$", false, 1 };
                yield return new object[] { "GroupDisplay", "Group 1", "^[a-z]+$", false, 1 };
                yield return new object[] { "Group 1", "Group 1", "^[a-z]+$", false, 2 };
            }
        }
    }
}
