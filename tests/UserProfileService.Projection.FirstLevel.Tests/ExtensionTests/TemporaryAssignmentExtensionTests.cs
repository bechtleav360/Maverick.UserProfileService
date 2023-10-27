using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Extensions;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.ExtensionTests
{
    public class TemporaryAssignmentExtensionTests
    {
        [Theory]
        [MemberData(nameof(GetStateTestArguments))]
        public void Get_state_should_work(
            DateTime? start,
            DateTime? end,
            TemporaryAssignmentState expectedState)
        {
            var assignment =
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id = "my-id-whatever",
                    LastModified = DateTime.UtcNow,
                    State = TemporaryAssignmentState.NotProcessed,
                    ProfileId = "profile-1",
                    Start = start,
                    End = end,
                    TargetId = "target-1",
                    ProfileType = ObjectType.User,
                    TargetType = ObjectType.Group
                };

            assignment.UpdateState();

            Assert.Equal(expectedState, assignment.State);
        }

        public static IEnumerable<object[]> GetStateTestArguments()
        {
            yield return new object[]
            {
                new DateTime(
                    2000,
                    12,
                    31,
                    18,
                    00,
                    25,
                    DateTimeKind.Utc),
                new DateTime(
                    2010,
                    1,
                    5,
                    9,
                    41,
                    53,
                    19,
                    DateTimeKind.Utc),
                TemporaryAssignmentState.Inactive
            };

            yield return new object[]
            {
                null,
                new DateTime(
                    2010,
                    1,
                    5,
                    9,
                    41,
                    53,
                    19,
                    DateTimeKind.Utc),
                TemporaryAssignmentState.Inactive
            };

            yield return new object[]
            {
                new DateTime(
                    2020,
                    8,
                    9,
                    10,
                    41,
                    49,
                    DateTimeKind.Utc),
                DateTime.MaxValue.ToUniversalTime(),
                TemporaryAssignmentState.Active
            };

            yield return new object[]
            {
                new DateTime(
                    2020,
                    8,
                    9,
                    10,
                    41,
                    49,
                    DateTimeKind.Utc),
                null,
                TemporaryAssignmentState.Active
            };

            yield return new object[]
            {
                new DateTime(
                    2018,
                    11,
                    25,
                    14,
                    8,
                    53,
                    DateTimeKind.Utc),
                new DateTime(
                    3000,
                    6,
                    12,
                    0,
                    48,
                    7,
                    DateTimeKind.Utc),
                TemporaryAssignmentState.ActiveWithExpiration
            };

            yield return new object[]
            {
                null,
                new DateTime(
                    3000,
                    6,
                    12,
                    0,
                    48,
                    7,
                    DateTimeKind.Utc),
                TemporaryAssignmentState.ActiveWithExpiration
            };

            yield return new object[]
            {
                new DateTime(
                    2900,
                    5,
                    17,
                    23,
                    58,
                    21,
                    DateTimeKind.Utc),
                new DateTime(
                    3000,
                    10,
                    23,
                    20,
                    48,
                    7,
                    DateTimeKind.Utc),
                TemporaryAssignmentState.NotProcessed
            };
        }
    }
}
