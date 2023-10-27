using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Tests.Extensions;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using CommonResolved = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;
using InitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.MethodTest
{
    public static class MemberDataForEventHelper
    {
#region RefenreceGroupTests

        internal static IStreamNameResolver StreamNameResolver =
            FirstLevelHandlerTestsPreparationHelper.GetFirstLevelNameResolver();

        internal static string GroupId = "EAAE024E-9F91-460D-A2FA-D48323FE7331";
        internal static DateTime StartDate = DateTime.Now.ToUniversalTime();

        internal static ObjectIdent GroupTestRelatedGroup = new ObjectIdent(
            "04AA2AC1-E7BB-45BA-B8C0-746B3891BC40",
            ObjectType.Group);

        internal static ObjectIdent GroupTestRelatedUser = new ObjectIdent(
            "2234D9DF-ABF8-45BC-8796-6C0BA5DBD4C7",
            ObjectType.User);

        internal static ObjectIdent GroupTestRelatedFunction = new ObjectIdent(
            "60137FFD-5E9B-41B5-BECB-42AF06B2A136",
            ObjectType.Function);

        internal static ObjectIdent GroupTestRelatedRole = new ObjectIdent(
            "085EF35B-5A89-4475-A286-F470DA466548",
            ObjectType.Role);

        internal static EventMetaData EventPropertiesChangedMetaDataGroup = new EventMetaData
                                                                            {
                                                                                Initiator =
                                                                                    new EventInitiator
                                                                                    {
                                                                                        Id =
                                                                                            "E1081E1C-EE1C-471D-A627-E5508BE7CF98",
                                                                                        Type = CommonResolved.System
                                                                                    },
                                                                                Timestamp = StartDate,
                                                                                CorrelationId =
                                                                                    "8D4955FE-459C-49DF-8BCD-A262D32695E4",
                                                                                ProcessId =
                                                                                    "9EB48E1E-5E64-4EF5-8381-7E875FE491A8",
                                                                                VersionInformation = 1,
                                                                                HasToBeInverted = false,
                                                                                RelatedEntityId = "1"
                                                                            };

        internal static ProfilePropertiesChangedEvent PropertiesChangedGroupEvent =
            new ProfilePropertiesChangedEvent
            {
                Payload = new PropertiesUpdatedPayload
                          {
                              Id = GroupTestRelatedGroup.Id,
                              Properties = new Dictionary<string, object>
                                           {
                                               { "Name", "ChangedNameOfGroup" }
                                           }
                          },
                Initiator = new EventSourcing.Abstractions.Models.EventInitiator
                            {
                                Id = "E1081E1C-EE1C-471D-A627-E5508BE7CF98",
                                Type = InitiatorType.System
                            },
                CorrelationId = "8D4955FE-459C-49DF-8BCD-A262D32695E4",
                Timestamp = StartDate,
                ProfileKind = ProfileKind.Group,
                VersionInformation = 2,
                RequestSagaId = "9EB48E1E-5E64-4EF5-8381-7E875FE491A8",
                MetaData = EventPropertiesChangedMetaDataGroup.CloneEventDate()
            };

        internal static PropertiesChanged PropertiesChanged = new PropertiesChanged
                                                              {
                                                                  Id = GroupId,
                                                                  Properties = PropertiesChangedGroupEvent.Payload
                                                                      .Properties,
                                                                  ObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                                                                  MetaData = EventPropertiesChangedMetaDataGroup
                                                                      .CloneEventDate()
                                                              };

        internal static EventTuple ReferenceGroupMemberEventTupleResult = new EventTuple
                                                                          {
                                                                              Event = PropertiesChanged.CloneEvent()
                                                                                  .SetRelatedContext(
                                                                                      PropertiesChangedContext.Members)
                                                                                  .SetRelatedEntityId(
                                                                                      StreamNameResolver.GetStreamName(
                                                                                          GroupTestRelatedGroup)),
                                                                              TargetStream =
                                                                                  StreamNameResolver.GetStreamName(
                                                                                      GroupTestRelatedGroup)
                                                                          };

        internal static EventTuple ReferenceGroupMemberOfEventTupleResult = new EventTuple
                                                                            {
                                                                                Event = PropertiesChanged.CloneEvent()
                                                                                    .SetRelatedContext(
                                                                                        PropertiesChangedContext
                                                                                            .MemberOf)
                                                                                    .SetRelatedEntityId(
                                                                                        StreamNameResolver
                                                                                            .GetStreamName(
                                                                                                GroupTestRelatedGroup)),
                                                                                TargetStream =
                                                                                    StreamNameResolver.GetStreamName(
                                                                                        GroupTestRelatedGroup)
                                                                            };

        internal static EventTuple ReferenceUserMemberEventTupleResult = new EventTuple
                                                                         {
                                                                             Event = PropertiesChanged.CloneEvent()
                                                                                 .SetRelatedContext(
                                                                                     PropertiesChangedContext
                                                                                         .MemberOf)
                                                                                 .SetRelatedEntityId(
                                                                                     StreamNameResolver
                                                                                         .GetStreamName(
                                                                                             GroupTestRelatedUser)),
                                                                             TargetStream =
                                                                                 StreamNameResolver.GetStreamName(
                                                                                     GroupTestRelatedUser)
                                                                         };

        internal static EventTuple ReferenceFunctionMemberEventTupleResult = new EventTuple
                                                                             {
                                                                                 Event = PropertiesChanged.CloneEvent()
                                                                                     .SetRelatedContext(
                                                                                         PropertiesChangedContext
                                                                                             .LinkedProfiles)
                                                                                     .SetRelatedEntityId(
                                                                                         StreamNameResolver
                                                                                             .GetStreamName(
                                                                                                 GroupTestRelatedFunction)),
                                                                                 TargetStream =
                                                                                     StreamNameResolver.GetStreamName(
                                                                                         GroupTestRelatedFunction)
                                                                             };

        internal static EventTuple ReferenceRoleMemberEventTupleResult = new EventTuple
                                                                         {
                                                                             Event = PropertiesChanged.CloneEvent()
                                                                                 .SetRelatedContext(
                                                                                     PropertiesChangedContext
                                                                                         .LinkedProfiles)
                                                                                 .SetRelatedEntityId(
                                                                                     StreamNameResolver
                                                                                         .GetStreamName(
                                                                                             GroupTestRelatedRole)),
                                                                             TargetStream =
                                                                                 StreamNameResolver.GetStreamName(
                                                                                     GroupTestRelatedRole)
                                                                         };

#endregion

#region RerenceUserTests

        internal static ObjectIdent UserTestRelatedGroup = new ObjectIdent(
            "D72C511A-5F11-4BF3-AF5E-DA35C9127866",
            ObjectType.Group);

        internal static ObjectIdent UserTestRelatedFunction = new ObjectIdent(
            "D31BE6F0-10EB-471A-9362-2645162F3BDD",
            ObjectType.Function);

        internal static ObjectIdent UserTestRelatedRole = new ObjectIdent(
            "67B6D94B-05DD-4868-9A6F-0935B934D57A",
            ObjectType.Role);

        internal static string UserId = "19D12B24-1AB7-48D3-B6DB-0DDC6B50021C";

        internal static EventMetaData EventPropertiesChangedMetaDataUser = new EventMetaData
                                                                           {
                                                                               Initiator =
                                                                                   new EventInitiator
                                                                                   {
                                                                                       Id =
                                                                                           "E1081E1C-EE1C-471D-A627-E5508BE7CF98",
                                                                                       Type = CommonResolved.System
                                                                                   },
                                                                               Timestamp = StartDate,
                                                                               CorrelationId =
                                                                                   "96441423-A578-41B8-B8A3-09EF2035B050",
                                                                               ProcessId =
                                                                                   "EF1631C0-8192-4A29-9344-E66277FDD902",
                                                                               VersionInformation = 1,
                                                                               HasToBeInverted = false,
                                                                               RelatedEntityId = "1"
                                                                           };

        internal static ProfilePropertiesChangedEvent PropertiesChangedUserEvent =
            new ProfilePropertiesChangedEvent
            {
                Payload = new PropertiesUpdatedPayload
                          {
                              Id = UserId,
                              Properties = new Dictionary<string, object>
                                           {
                                               { "Name", "ChangedNameOfGroup" }
                                           }
                          },
                Initiator = new EventSourcing.Abstractions.Models.EventInitiator
                            {
                                Id = "E1081E1C-EE1C-471D-A627-E5508BE7CF98",
                                Type = InitiatorType.System
                            },
                CorrelationId = "96441423-A578-41B8-B8A3-09EF2035B050",
                Timestamp = StartDate,
                ProfileKind = ProfileKind.User,
                VersionInformation = 2,
                RequestSagaId = "EF1631C0-8192-4A29-9344-E66277FDD902",
                MetaData = EventPropertiesChangedMetaDataUser.CloneEventDate()
            };

        internal static PropertiesChanged PropertiesChangedUser = new PropertiesChanged
                                                                  {
                                                                      Id = UserId,
                                                                      Properties = PropertiesChangedUserEvent.Payload
                                                                          .Properties,
                                                                      ObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType
                                                                          .User,
                                                                      MetaData = EventPropertiesChangedMetaDataUser
                                                                          .CloneEventDate()
                                                                  };

        internal static EventTuple GroupMemberEventTupleResult = new EventTuple
                                                                 {
                                                                     Event = PropertiesChangedUser.CloneEvent()
                                                                         .SetRelatedContext(
                                                                             PropertiesChangedContext
                                                                                 .Members)
                                                                         .SetRelatedEntityId(
                                                                             StreamNameResolver
                                                                                 .GetStreamName(UserTestRelatedGroup)),
                                                                     TargetStream =
                                                                         StreamNameResolver.GetStreamName(
                                                                             UserTestRelatedGroup)
                                                                 };

        internal static EventTuple FunctionMemberEventTupleResult = new EventTuple
                                                                    {
                                                                        Event = PropertiesChangedUser.CloneEvent()
                                                                            .SetRelatedContext(
                                                                                PropertiesChangedContext
                                                                                    .LinkedProfiles)
                                                                            .SetRelatedEntityId(
                                                                                StreamNameResolver
                                                                                    .GetStreamName(
                                                                                        UserTestRelatedFunction)),
                                                                        TargetStream =
                                                                            StreamNameResolver.GetStreamName(
                                                                                UserTestRelatedFunction)
                                                                    };

        internal static EventTuple RoleMemberEventTupleResult = new EventTuple
                                                                {
                                                                    Event = PropertiesChangedUser.CloneEvent()
                                                                        .SetRelatedContext(
                                                                            PropertiesChangedContext
                                                                                .LinkedProfiles)
                                                                        .SetRelatedEntityId(
                                                                            StreamNameResolver
                                                                                .GetStreamName(UserTestRelatedRole)),
                                                                    TargetStream =
                                                                        StreamNameResolver.GetStreamName(
                                                                            UserTestRelatedRole)
                                                                };

#endregion

#region RerenceOrganizationTests

        internal static string OrganizationId = "3628F937-770F-4487-83E2-252A914F279B";

        internal static EventMetaData EventPropertiesChangedMetaDataOrganization = new EventMetaData
            {
                Initiator =
                    new EventInitiator
                    {
                        Id =
                            "99A7B413-9118-4FD3-B1B4-13F66ABD9D49",
                        Type = CommonResolved.System
                    },
                Timestamp = StartDate,
                CorrelationId =
                    "A5DD133B-4660-4BE1-BE07-C9407E23DBC1",
                ProcessId =
                    "F8FFCEC4-789D-41AA-BC80-CA5157622BCC",
                VersionInformation = 1,
                HasToBeInverted = false,
                RelatedEntityId = "1"
            };

        internal static ObjectIdent OrganizationTestRelatedOrganization = new ObjectIdent(
            "C8277A66-E68F-4BB0-984F-1F331339F5E7",
            ObjectType.Organization);

        internal static ProfilePropertiesChangedEvent PropertiesChangedOrganizationEvent =
            new ProfilePropertiesChangedEvent
            {
                Payload = new PropertiesUpdatedPayload
                          {
                              Id = UserId,
                              Properties = new Dictionary<string, object>
                                           {
                                               { "Name", "ChangedNameOfOrganization" }
                                           }
                          },
                Initiator = new EventSourcing.Abstractions.Models.EventInitiator
                            {
                                Id = "99A7B413-9118-4FD3-B1B4-13F66ABD9D49",
                                Type = InitiatorType.System
                            },
                CorrelationId = "A5DD133B-4660-4BE1-BE07-C9407E23DBC1",
                Timestamp = StartDate,
                ProfileKind = ProfileKind.Organization,
                VersionInformation = 2,
                RequestSagaId = "F8FFCEC4-789D-41AA-BC80-CA5157622BCC",
                MetaData = EventPropertiesChangedMetaDataUser.CloneEventDate()
            };

        internal static PropertiesChanged PropertiesChangedOrganization = new PropertiesChanged
                                                                          {
                                                                              Id = OrganizationId,
                                                                              Properties =
                                                                                  PropertiesChangedOrganizationEvent
                                                                                      .Payload
                                                                                      .Properties,
                                                                              ObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType
                                                                                  .Organization,
                                                                              MetaData =
                                                                                  EventPropertiesChangedMetaDataOrganization
                                                                                      .CloneEventDate()
                                                                          };

        internal static EventTuple OrganizationMemberEventTupleResult = new EventTuple
                                                                        {
                                                                            Event = PropertiesChangedOrganization
                                                                                .CloneEvent()
                                                                                .SetRelatedContext(
                                                                                    PropertiesChangedContext
                                                                                        .MemberOf)
                                                                                .SetRelatedEntityId(
                                                                                    StreamNameResolver
                                                                                        .GetStreamName(
                                                                                            OrganizationTestRelatedOrganization)),
                                                                            TargetStream =
                                                                                StreamNameResolver.GetStreamName(
                                                                                    OrganizationTestRelatedOrganization)
                                                                        };

        internal static EventTuple OrganizationMemberOfEventTupleResult = new EventTuple
                                                                          {
                                                                              Event = PropertiesChangedOrganization
                                                                                  .CloneEvent()
                                                                                  .SetRelatedContext(
                                                                                      PropertiesChangedContext
                                                                                          .Members)
                                                                                  .SetRelatedEntityId(
                                                                                      StreamNameResolver
                                                                                          .GetStreamName(
                                                                                              OrganizationTestRelatedOrganization)),
                                                                              TargetStream =
                                                                                  StreamNameResolver.GetStreamName(
                                                                                      OrganizationTestRelatedOrganization)
                                                                          };

#endregion

#region MemberData
        public static IEnumerable<object[]> GroupTestMemberData =>
            new List<object[]>
            {
                new object[]
                {
                    GroupId,
                    GroupTestRelatedGroup,
                    PropertiesChangedRelation.MemberOf,
                    PropertiesChangedGroupEvent,
                    ReferenceGroupMemberEventTupleResult
                },
                new object[]
                {
                    GroupId,
                    GroupTestRelatedGroup,
                    PropertiesChangedRelation.Member,
                    PropertiesChangedGroupEvent,
                    ReferenceGroupMemberOfEventTupleResult
                },
                new object[]
                {
                    GroupId,
                    GroupTestRelatedUser,
                    PropertiesChangedRelation.MemberOf,
                    PropertiesChangedGroupEvent,
                    ReferenceUserMemberEventTupleResult
                },
                new object[]
                {
                    GroupId,
                    GroupTestRelatedFunction,
                    PropertiesChangedRelation.Member,
                    PropertiesChangedGroupEvent,
                    ReferenceFunctionMemberEventTupleResult
                },
                new object[]
                {
                    GroupId,
                    GroupTestRelatedRole,
                    PropertiesChangedRelation.Member,
                    PropertiesChangedGroupEvent,
                    ReferenceRoleMemberEventTupleResult
                }
            };

        public static IEnumerable<object[]> UserTestMemberData =>
            new List<object[]>
            {
                new object[]
                {
                    UserId,
                    UserTestRelatedGroup,
                    PropertiesChangedUserEvent,
                    GroupMemberEventTupleResult
                },
                new object[]
                {
                    UserId,
                    UserTestRelatedFunction,
                    PropertiesChangedUserEvent,
                    FunctionMemberEventTupleResult
                },
                new object[]
                {
                    UserId,
                    UserTestRelatedRole,
                    PropertiesChangedUserEvent,
                    RoleMemberEventTupleResult
                }
            };

        public static IEnumerable<object[]> OrganizationTestMemberData =>
            new List<object[]>
            {
                new object[]
                {
                    OrganizationId,
                    OrganizationTestRelatedOrganization,
                    PropertiesChangedRelation.Member,
                    PropertiesChangedOrganizationEvent,
                    OrganizationMemberEventTupleResult
                },
                new object[]
                {
                    OrganizationId,
                    OrganizationTestRelatedOrganization,
                    PropertiesChangedRelation.MemberOf,
                    PropertiesChangedOrganizationEvent,
                    OrganizationMemberOfEventTupleResult
                }
            };

#endregion
    }
}
