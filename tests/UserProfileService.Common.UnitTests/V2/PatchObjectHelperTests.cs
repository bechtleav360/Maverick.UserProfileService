using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Common.Tests.Utilities.TestModels;
using UserProfileService.Common.UnitTests.V2.TestArguments;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Utilities;
using Xunit;

namespace UserProfileService.Common.UnitTests.V2
{
    public class PatchObjectHelperTests
    {
        [Fact]
        public void Apply_changes_on_object_and_set_double_as_string_to_double_property_should_fail()
        {
            // arrange
            (Employee toBeModified, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.SetDoubleAsStringToDouble();

            // act & assert
            Assert.Throws<NotValidException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_and_set_null_to_value_type_property_should_fail()
        {
            // arrange
            (Employee toBeModified, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.SetNullToValueType();

            // act & assert
            Assert.Throws<NotValidException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_and_set_string_to_double_property_should_fail()
        {
            // arrange
            (Employee toBeModified, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.SetStringToDouble();

            // act & assert
            Assert.Throws<NotValidException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_but_without_providing_object_should_fail()
        {
            // arrange
            IReadOnlyDictionary<string, object> changeSet = new Dictionary<string, object>
            {
                { nameof(Employee.FirstName), "test" }
            };

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        null,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_but_without_setting_any_valid_changes_should_fail()
        {
            // arrange
            Employee toBeModified = SampleDataHelper.GetEmployees().First();

            IReadOnlyDictionary<string, object> changeSet = new Dictionary<string, object>
            {
                { "notNow", "true" },
                { "rockMe", DateTime.UtcNow }
            };

            // act & assert
            Assert.Throws<ArgumentException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_but_without_setting_changes_cause_of_null_should_fail()
        {
            // arrange
            Employee toBeModified = SampleDataHelper.GetEmployees().First();

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        null,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_but_without_setting_changes_should_fail()
        {
            // arrange
            Employee toBeModified = SampleDataHelper.GetEmployees().First();
            IReadOnlyDictionary<string, object> changeSet = new Dictionary<string, object>();

            // act & assert
            Assert.Throws<ArgumentException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        toBeModified,
                        changeSet,
                        new MockMetadataTimestampEvent()));
        }

        [Fact]
        public void Apply_changes_on_object_using_arrays_on_IList_properties_should_work()
        {
            // arrange
            (Employee toBeModified, Employee referenceValueAfterPatch, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.SetArrayToIListPropertyType();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                toBeModified,
                changeSet,
                new MockMetadataTimestampEvent());

            // assert
            toBeModified.Should().BeEquivalentTo(referenceValueAfterPatch);
        }

        [Fact]
        public void Apply_changes_on_object_using_nullable_value_types_should_work()
        {
            // arrange
            (Employee toBeModified, Employee referenceValueAfterPatch, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.ChangingNullableValueTypes();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                toBeModified,
                changeSet,
                new MockMetadataTimestampEvent());

            // assert
            toBeModified.Should().BeEquivalentTo(referenceValueAfterPatch);
        }

        [Fact]
        public void Apply_changes_on_object_using_one_invalid_property_name_together_with_valid_should_work()
        {
            // arrange
            (Employee toBeModified, Employee referenceValueAfterPatch, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.ValidAndInvalidParameterChangesTogether();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                toBeModified,
                changeSet,
                new MockMetadataTimestampEvent());

            // assert
            toBeModified.Should().BeEquivalentTo(referenceValueAfterPatch);
        }

        [Fact]
        public void Apply_changes_on_object_with_different_property_name_spelling()
        {
            // arrange
            (Employee toBeModified, Employee referenceValueAfterPatch, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.DifferentPropertyNameSpelling();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                toBeModified,
                changeSet,
                new MockMetadataTimestampEvent());

            // assert
            toBeModified.Should().BeEquivalentTo(referenceValueAfterPatch);
        }

        [Fact]
        public void Apply_simple_changes_on_object_should_work()
        {
            // arrange
            (Employee toBeModified, Employee referenceValueAfterPatch, IReadOnlyDictionary<string, object> changeSet)
                = PatchObjectTestArguments.SimpleOverwritePropertyValues();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                toBeModified,
                changeSet,
                new MockMetadataTimestampEvent());

            // assert
            toBeModified.Should().BeEquivalentTo(referenceValueAfterPatch);
        }

        [Fact]
        public void Apply_simple_changes_on_object_with_profile_timestamp_should_work()
        {
            // arrange
            var group = new Group
            {
                Name = "Group 1",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            IReadOnlyDictionary<string, object> changeSet = new Dictionary<string, object>
            {
                { nameof(Group.Name), "Group 1 Edit" },
                { nameof(Group.UpdatedAt), DateTime.UtcNow }
            };

            var testEvent = new MockMetadataTimestampEvent();

            // act
            PatchObjectHelpers.ApplyPropertyChanges(
                group,
                changeSet,
                testEvent);

            // assert
            Assert.Equal(group.Name, changeSet[nameof(Group.Name)].ToString());
            Assert.Equal(group.UpdatedAt, testEvent.MetaData.Timestamp);
        }

        [Fact]
        public void Apply_changes_on_object_with_profile_timestamp_but_without_setting_event_cause_of_null_should_fail()
        {
            // arrange
            var group = new Group
            {
                Name = "Group 1",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            IReadOnlyDictionary<string, object> changeSet = new Dictionary<string, object>
            {
                { nameof(Group.Name), "Group 1 Edit" },
                { nameof(Group.UpdatedAt), DateTime.UtcNow }
            };

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () =>
                    PatchObjectHelpers.ApplyPropertyChanges(
                        group,
                        changeSet,
                        null));
        }
    }

    public class MockMetadataTimestampEvent : IUserProfileServiceEvent
    {
        public string Type => GetType().Name;

        public string EventId { get; set; }

        public EventMetaData MetaData { get; set; }

        public MockMetadataTimestampEvent(DateTime? dateTime = null)
        {
            MetaData = new EventMetaData
            {
                Timestamp = dateTime ?? DateTime.UtcNow
            };
        }
    }
}
