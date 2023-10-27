using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using UserProfileService.Projection.FirstLevel.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.MethodTest
{
    public class PropertiesChangedMembersEventHelperTests
    {
        private readonly IFirstLevelEventTupleCreator _firstLevelTupleCreator = FirstLevelHandlerTestsPreparationHelper.GetFirstLevelEventTupleCreator();

        [Theory]
        [MemberData(
            nameof(MemberDataForEventHelper.GroupTestMemberData),
            MemberType = typeof(MemberDataForEventHelper))]
        public void GroupMemberTupleEventsTest(
            string groupId,
            ObjectIdent relatedEntity,
            PropertiesChangedRelation propertiesContext,
            ProfilePropertiesChangedEvent propertiesChangedEvent,
            EventTuple expectedEventTuple)
        {
            EventTuple eventTupleResult = PropertiesChangedMembersEventHelper.HandleGroupAsReference(
                groupId,
                relatedEntity,
                propertiesContext,
                propertiesChangedEvent,
                _firstLevelTupleCreator);

            eventTupleResult.Should()
                            .BeEquivalentTo(
                                expectedEventTuple,
                                opt => opt.Excluding(p => p.Event.EventId)
                                          .Excluding(p => p.Event.MetaData.Batch)
                                          .Excluding(p => p.Event.MetaData.Timestamp)
                                          .RespectingRuntimeTypes());
        }

        [Theory]
        [MemberData(
            nameof(MemberDataForEventHelper.UserTestMemberData),
            MemberType = typeof(MemberDataForEventHelper))]
        public void UserMemberTupleEventsTest(
            string userId,
            ObjectIdent relatedEntity,
            ProfilePropertiesChangedEvent propertiesChangedEvent,
            EventTuple expectedEventTuple)
        {
            EventTuple eventTupleResult = PropertiesChangedMembersEventHelper.HandleUserAsReference(
                userId,
                relatedEntity,
                propertiesChangedEvent,
                _firstLevelTupleCreator);

            eventTupleResult.Should()
                            .BeEquivalentTo(
                                expectedEventTuple,
                                opt => opt.Excluding(p => p.Event.EventId)
                                          .Excluding(p => p.Event.MetaData.Batch)
                                          .Excluding(p => p.Event.MetaData.Timestamp)
                                          .RespectingRuntimeTypes());
        }

        [Theory]
        [MemberData(
            nameof(MemberDataForEventHelper.OrganizationTestMemberData),
            MemberType = typeof(MemberDataForEventHelper))]
        public void OrganizationMemberTupleEventsTest(
            string organizationId,
            ObjectIdent relatedEntity,
            PropertiesChangedRelation propertiesContext,
            ProfilePropertiesChangedEvent propertiesChangedEvent,
            EventTuple expectedEventTuple)
        {
            EventTuple eventTupleResult = PropertiesChangedMembersEventHelper.HandleOrganizationAsReference(
                organizationId,
                relatedEntity,
                propertiesContext,
                propertiesChangedEvent,
                _firstLevelTupleCreator);

            eventTupleResult.Should()
                            .BeEquivalentTo(
                                expectedEventTuple,
                                opt => opt.Excluding(p => p.Event.EventId)
                                          .Excluding(p => p.Event.MetaData.Batch)
                                          .Excluding(p => p.Event.MetaData.Timestamp)
                                          .RespectingRuntimeTypes());
        }
    }
}
