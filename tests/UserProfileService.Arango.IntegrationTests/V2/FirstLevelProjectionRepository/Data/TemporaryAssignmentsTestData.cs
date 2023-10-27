using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    public static class TemporaryAssignmentsTestData
    {
        private static DateTime Normalize(DateTime dateTime)
        {
            DateTime dt = dateTime.ToUniversalTime();

            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                dt.Hour,
                0,
                0,
                DateTimeKind.Utc);
        }

        [SeedGeneralData(typeof(FirstLevelProjectionTemporaryAssignment), TestType.ReadTest)]
        public static class ExistingTemporaryAssignments
        {
            // reference date/year < 2050 ;)
            //                                      [--- (1) ---]
            //       [---- (2) ---]
            //                     [ ------ (3) ---]
            //     [-------------------- (4) -------------------------]
            //  <------------- past ---- | now | ---- future ---->
            public static List<FirstLevelProjectionTemporaryAssignment> Assignments =>
                new List<FirstLevelProjectionTemporaryAssignment>
                {
                    // case (1)
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "temp-assignment-123-future",
                        Start = new DateTime(
                            2050,
                            1,
                            12,
                            22,
                            5,
                            0,
                            DateTimeKind.Utc),
                        End = new DateTime(
                            2060,
                            1,
                            1,
                            9,
                            0,
                            0,
                            DateTimeKind.Utc),
                        ProfileId = "user-1",
                        ProfileType = ObjectType.User,
                        TargetId = "group-1",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.NotProcessed,
                        NotificationStatus = NotificationStatus.NoneSent,
                        LastModified = new DateTime(
                            2022,
                            1,
                            1,
                            14,
                            53,
                            21,
                            0,
                            DateTimeKind.Utc)
                    },
                    // case (2)
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "temp-assignment-456-past",
                        Start = new DateTime(
                            2000,
                            6,
                            7,
                            10,
                            18,
                            0,
                            DateTimeKind.Utc),
                        End = new DateTime(
                            2006,
                            6,
                            9,
                            23,
                            3,
                            0,
                            DateTimeKind.Utc),
                        ProfileId = "group-2",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-old-1",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.Inactive,
                        NotificationStatus = NotificationStatus.BothSent,
                        LastModified = new DateTime(
                            2004,
                            1,
                            1,
                            0,
                            0,
                            0,
                            451,
                            DateTimeKind.Utc)
                    },
                    // case (2) --> not processed yet
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "out-assignment-456-past-not-processed-yet",
                        Start = new DateTime(
                            2000,
                            6,
                            7,
                            10,
                            18,
                            0,
                            DateTimeKind.Utc),
                        End = new DateTime(
                            2006,
                            6,
                            9,
                            23,
                            3,
                            0,
                            DateTimeKind.Utc),
                        ProfileId = "group-2",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-old-1",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.ActiveWithExpiration,
                        NotificationStatus = NotificationStatus.ActivationSent,
                        LastModified = new DateTime(
                            2004,
                            1,
                            1,
                            0,
                            0,
                            0,
                            451,
                            DateTimeKind.Utc)
                    },
                    // case (3) 
                    // Here the assignment is not processed and should change the state
                    // from notProcessed --> activeWithExpiration
                    // Also the NotificationStatus changes from NotSent --> ActivationSent
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "out_temp-assignment-456-still-active-not-process-yet",
                        Start = new DateTime(
                            2020,
                            1,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        End = new DateTime(
                            2040,
                            12,
                            31,
                            23,
                            59,
                            59,
                            DateTimeKind.Utc),
                        ProfileId = "group-sub-bosses",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-super-power",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.NotProcessed,
                        NotificationStatus = NotificationStatus.NoneSent,
                        LastModified = new DateTime(
                            2016,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc)
                    },
                    // case (3) --> not processed yet
                    // Here the assignment is not active and should change the state from
                    // activeWithExpiration --> inactive
                    // The NotificationStatus changes from NotSent --> BothSend
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "out_temp-assignment-456-still-already-inactive-but-process-once",
                        Start = new DateTime(
                            2020,
                            1,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        End = DateTime.Today.ToUniversalTime(),
                        ProfileId = "group-sub-bosses",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-super-power",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.ActiveWithExpiration,
                        NotificationStatus = NotificationStatus.ActivationSent,
                        LastModified = new DateTime(
                            2016,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc)
                    },
                    // case (4) --> processed 
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "temp-assignment-456-forever-active",
                        Start = new DateTime(
                            2015,
                            1,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        End = null,
                        ProfileId = "group-sub-bosses",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-super-power",
                        TargetType = ObjectType.Group,
                        LastModified = new DateTime(
                            2015,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        NotificationStatus = NotificationStatus.ActivationSent,
                        State = TemporaryAssignmentState.Active
                    },
                    // case (4) --> not processed yet
                    // Here the assignment is not active and should change the state from
                    // notProcessed --> active
                    // The NotificationStatus changes from NotSent --> ActivationSent (This notification state won't change,
                    // because the assignment will last forever)
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "out-assignment-456-forever-active-not-processed-yet",
                        Start = new DateTime(
                            2015,
                            1,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        End = null,
                        ProfileId = "group-sub-bosses",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-super-power",
                        TargetType = ObjectType.Group,
                        LastModified = new DateTime(
                            2015,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc),
                        NotificationStatus = NotificationStatus.NoneSent,
                        State = TemporaryAssignmentState.NotProcessed
                    },
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id = "temp-assignment-456-past",
                        Start = new DateTime(
                            2000,
                            6,
                            7,
                            10,
                            18,
                            0,
                            DateTimeKind.Utc),
                        End = new DateTime(
                            2006,
                            6,
                            9,
                            23,
                            3,
                            0,
                            DateTimeKind.Utc),
                        ProfileId = "group-2",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-old-1",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState.Inactive,
                        NotificationStatus = NotificationStatus.BothSent,
                        LastModified = new DateTime(
                            2004,
                            1,
                            1,
                            0,
                            0,
                            0,
                            451,
                            DateTimeKind.Utc)
                    }
                };
        }

        [SeedGeneralData(typeof(FirstLevelProjectionTemporaryAssignment), TestType.WriteTest)]
        public static class WriteTestData
        {
            public static List<FirstLevelProjectionTemporaryAssignment> Assignments =>
                new List<FirstLevelProjectionTemporaryAssignment>
                {
                    new FirstLevelProjectionTemporaryAssignment
                    {
                        Id =
                            "write-tests-temp-assignment-1",
                        Start = Normalize(DateTime.UtcNow.AddYears(1)),
                        End = null,
                        ProfileId = "group-maggots",
                        ProfileType = ObjectType.Group,
                        TargetId = "group-ppl-like-gods",
                        TargetType = ObjectType.Group,
                        State = TemporaryAssignmentState
                            .NotProcessed,
                        LastModified =
                            DateTime.UtcNow
                    }
                };
        }
    }
}
