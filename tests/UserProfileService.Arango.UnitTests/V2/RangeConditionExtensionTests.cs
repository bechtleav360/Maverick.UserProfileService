using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Arango.UnitTests.V2
{
    public class RangeConditionExtensionTests
    {
        private static readonly RangeCondition _current =
            new RangeCondition
            {
                Start = DateTime.Today.ToUniversalTime().AddMonths(-6),
                End = DateTime.Today.ToUniversalTime().AddYears(1)
            };

        private static readonly RangeCondition _future =
            new RangeCondition
            {
                Start = new DateTime(
                    2050,
                    1,
                    1,
                    8,
                    0,
                    0,
                    DateTimeKind.Utc),
                End = new DateTime(
                    2070,
                    1,
                    1,
                    8,
                    0,
                    0,
                    DateTimeKind.Utc)
            };

        private static readonly RangeCondition _past =
            new RangeCondition
            {
                Start = new DateTime(
                    2000,
                    12,
                    26,
                    5,
                    0,
                    0,
                    DateTimeKind.Utc),
                End = new DateTime(
                    2020,
                    1,
                    1,
                    10,
                    30,
                    0,
                    DateTimeKind.Utc)
            };

        private static readonly RangeCondition _currentUseless =
            new RangeCondition
            {
                Start = new DateTime(
                    2000,
                    12,
                    26,
                    5,
                    0,
                    0,
                    DateTimeKind.Utc),
                End = null
            };

        private static readonly RangeCondition _futureWithoutExpiration =
            new RangeCondition
            {
                Start = DateTime.UtcNow.AddMonths(1),
                End = null
            };

        [Theory]
        [MemberData(nameof(GetArgsForConvertToTempAssignmentTests))]
        public void Conversion_to_temporary_assignment_should_work(
            RangeCondition condition,
            TemporaryAssignmentState state,
            bool canBeConverted)
        {
            // act
            FirstLevelProjectionTemporaryAssignment converted = condition.ToTemporaryAssignment(
                "user-1",
                "group-1",
                ContainerType.Group);

            // assert
            if (!canBeConverted)
            {
                Assert.Null(converted);

                return;
            }

            converted.Should()
                .BeEquivalentTo(
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Start = condition.Start,
                        End = condition.End,
                        State = state,
                        ProfileId = "user-1",
                        TargetId = "group-1",
                        TargetType = ObjectType.Group
                    },
                    o =>
                        o.Excluding(a => a.ProfileType)
                            .Excluding(a => a.LastErrorMessage)
                            .Excluding(a => a.LastModified)
                            .Excluding(a => a.Id));
        }

        [Fact]
        public void Conversion_to_temporary_assignments_should_work()
        {
            // arrange
            List<RangeCondition> assignments = GetArgsForConvertToTempAssignmentTests()
                .Select(o => (RangeCondition)o[0])
                .ToList();

            List<FirstLevelProjectionTemporaryAssignment> expected = GetArgsForConvertToTempAssignmentTests()
                .Where(o => (bool)o[2])
                .Select(
                    o =>
                        new FirstLevelProjectionTemporaryAssignment
                        {
                            Start = ((RangeCondition)o[0]).Start,
                            End = ((RangeCondition)o[0]).End,
                            State = (TemporaryAssignmentState)o[1],
                            ProfileId = "user-1",
                            TargetId = "group-1",
                            TargetType = ObjectType.Group
                        })
                .ToList();

            // act
            IList<FirstLevelProjectionTemporaryAssignment> converted = assignments
                .ToTemporaryAssignments("user-1", "group-1", ContainerType.Group);

            // assert
            converted.Should()
                .BeEquivalentTo(
                    expected,
                    o =>
                        o.Excluding(a => a.ProfileType)
                            .Excluding(a => a.LastErrorMessage)
                            .Excluding(a => a.LastModified)
                            .Excluding(a => a.Id));
        }

        [Fact]
        public void Conversion_null_to_temporary_assignments_should_work()
        {
            // act
            IList<FirstLevelProjectionTemporaryAssignment> converted = ((List<RangeCondition>)null)
                .ToTemporaryAssignments("user-1", "group-1", ContainerType.Group);

            // assert
            Assert.NotNull(converted);

            converted.Should()
                .BeEquivalentTo(new List<FirstLevelProjectionTemporaryAssignment>());
        }

        public static IEnumerable<object[]> GetArgsForConvertToTempAssignmentTests()
        {
            yield return new object[] { _future, TemporaryAssignmentState.NotProcessed, true };
            yield return new object[] { _current, TemporaryAssignmentState.ActiveWithExpiration, true };
            yield return new object[] { _currentUseless, TemporaryAssignmentState.Active, false };
            yield return new object[] { _past, TemporaryAssignmentState.NotProcessed, false };
            yield return new object[] { _futureWithoutExpiration, TemporaryAssignmentState.NotProcessed, true };
        }
    }
}
