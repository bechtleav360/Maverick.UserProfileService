using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Extensions;
using Xunit;

namespace UserProfileService.Common.UnitTests.V2
{
    public class ValidationTests
    {
        private static QueryObject GetDefaultQueryObject(Action<QueryObject> changingAction)
        {
            var resultingObject = new QueryObject
            {
                Filter = new Filter
                {
                    CombinedBy = BinaryOperator.And,
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            BinaryOperator = BinaryOperator.Or,
                            FieldName = "fieldOne",
                            Operator = FilterOperator.Equals,
                            Values = new[] { "my_test account", "val~42" }
                        },
                        new Definitions
                        {
                            BinaryOperator = BinaryOperator.Or,
                            FieldName = "fieldTwo",
                            Operator = FilterOperator.GreaterThan,
                            Values = new[] { "23", "42" }
                        }
                    }
                },
                Limit = 100,
                Offset = 50,
                OrderedBy = "name",
                Search = "Test Development d’accord éphémère",
                SortOrder = SortOrder.Asc
            };

            changingAction?.Invoke(resultingObject);

            return resultingObject;
        }

        [Fact]
        public void ValidateQueryObjectShouldWork()
        {
            GetDefaultQueryObject(null).Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkCauseOfWrongLimit()
        {
            // limit less than 0 => should not work
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(o => o.Limit = -20)
                        .Validate());

            Assert.Contains("Limit", exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkCauseOfZeroLimit()
        {
            // limit less than 0 => should not work
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(o => o.Limit = 0)
                        .Validate());

            Assert.Contains("Limit", exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkCauseOfWrongOffset()
        {
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(o => o.Offset = -123)
                        .Validate());

            Assert.Contains("Offset", exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkWithZeroOffset()
        {
            GetDefaultQueryObject(o => o.Offset = 0)
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughSearchTextIsMissing()
        {
            // Search equals null => should work
            GetDefaultQueryObject(o => o.Search = null)
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkCauseOfEmptySearchText()
        {
            // Search equals empty string => should not work
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(o => o.Search = "")
                        .Validate());

            Assert.Contains("Search", exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkCauseOfInvalidSearchText()
        {
            // Search contains only whitespaces => should not work
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(o => o.Search = "   ")
                        .Validate());

            Assert.Contains("Search", exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughOrderByIsMissing()
        {
            // ordered by equals null => should work, it is optional
            GetDefaultQueryObject(
                    o =>
                        o.OrderedBy = null)
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughOrderByIsEmpty()
        {
            // ordered by equals empty string => should work, it is optional
            GetDefaultQueryObject(
                    o =>
                        o.OrderedBy = "")
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughOrderByIsWhitespace()
        {
            // ordered by contains only whitespaces should work
            GetDefaultQueryObject(
                    o =>
                        o.OrderedBy = "  ")
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughFilterDefinitionIsEmpty()
        {
            // empty filter object => search without filter - should work
            GetDefaultQueryObject(
                    o =>
                        o.Filter = new Filter())
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAlthoughFilterDefinitionIsNull()
        {
            // filter object is null => search without filter - should work
            GetDefaultQueryObject(
                    o =>
                        o.Filter = null)
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkFilterDefinitionHasNoDefinitions()
        {
            // filter contains empty definition => should fail - exception
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(
                            o => o.Filter = new Filter
                            {
                                Definition = new List<Definitions>
                                {
                                    new Definitions()
                                }
                            })
                        .Validate());

            var expected = new Regex("Filter.+Definition\\s*\\[\\d+\\].+FieldName.+Values", RegexOptions.Singleline);
            Assert.Matches(expected, exception.Message);
        }

        [Fact]
        public void ValidateQueryObjectShouldWorkAtLeastOneDefinitionValid()
        {
            // filter contains one "null"-ed definition aside one regular one => should work, because at least one definition has been set
            GetDefaultQueryObject(
                    o => o.Filter = new Filter
                    {
                        Definition = new List<Definitions>
                        {
                            null,
                            new Definitions
                            {
                                FieldName = "fieldOne",
                                Operator = FilterOperator.Equals,
                                Values = new[] { "my_test account", "val~42" },
                                BinaryOperator = BinaryOperator.Or
                            }
                        }
                    })
                .Validate();
        }

        [Fact]
        public void ValidateQueryObjectShouldNotWorkOnlyNullAsDefinitions()
        {
            // filter contains only "null"-ed definitions => should not work 
            var exception = Assert.Throws<ValidationException>(
                () =>
                    GetDefaultQueryObject(
                            o => o.Filter = new Filter
                            {
                                Definition = new List<Definitions>
                                {
                                    null,
                                    null,
                                    null
                                }
                            })
                        .Validate());

            var expected = new Regex(
                "Filter.+Definition\\s*\\[\\d+\\].+Instance.+not be null",
                RegexOptions.Singleline);

            Assert.Matches(expected, exception.Message);
        }

        [Fact]
        public void ValidateFilterShouldNotWorkDefinitionCollectionEmpty()
        {
            var testObject = new Filter
            {
                Definition = new List<Definitions>()
            };

            var exception = Assert.Throws<ValidationException>(
                () => Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Matches("Definition.*empty", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldNotWorkMissingFieldName()
        {
            // filter contains "incomplete" definition - field name is empty => should not work
            var testObject = new Definitions
            {
                Values = new[] { "test" },
                FieldName = " "
            };

            var exception = Assert.Throws<ValidationException>(
                () =>
                    Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Contains("FieldName", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldNotWorkFieldNameNull()
        {
            // filter contains "incomplete" definition - field name is null => should not work
            var testObject = new Definitions
            {
                Values = new[] { "test" },
                FieldName = null
            };

            var exception = Assert.Throws<ValidationException>(
                () =>
                    Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Contains("FieldName", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldNotWorkMissingValues()
        {
            // filter contains "incomplete" definition - values array only contains null, empty strings or whitespace strings => should not work
            var testObject = new Definitions
            {
                Values = new[] { "", null, "  " },
                FieldName = "GodMode"
            };

            var exception = Assert.Throws<ValidationException>(
                () =>
                    Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Contains("Values", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldNotWorkValueArrayEmpty()
        {
            // filter contains "incomplete" definition - values array is empty => should not work
            var testObject = new Definitions
            {
                Values = new string[0],
                FieldName = "GodMode"
            };

            var exception = Assert.Throws<ValidationException>(
                () =>
                    Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Contains("Values", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldNotWorkValueArrayNull()
        {
            // filter contains "incomplete" definition - values array equals null => should not work
            var testObject = new Definitions
            {
                Values = null,
                FieldName = "GodMode"
            };

            var exception = Assert.Throws<ValidationException>(
                () =>
                    Validator.ValidateObject(testObject, new ValidationContext(testObject), true));

            Assert.Contains("Values", exception.Message);
        }

        [Fact]
        public void ValidateDefinitionShouldWorkAsAtLeastOneValidElementInValueArray()
        {
            // filter contains "incomplete" definition - values array contains null, empty strings, whitespace strings, but one "valid" string => should work, because at least one value is valid.
            var testObject = new Definitions
            {
                FieldName = "fieldOne",
                Values = new[] { null, "", "   ", "good/Value" }
            };

            Validator.ValidateObject(testObject, new ValidationContext(testObject), true);
        }
    }
}
