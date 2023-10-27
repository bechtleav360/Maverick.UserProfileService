using System;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;

namespace UserProfileService.Projection.Common.Tests.Helpers
{
    public static class AssertionHelpers
    {
        public static void AssertModelIsSimilarToCreatedEvent(
            object model,
            IUserProfileServiceEvent createdEvent)
        {
            switch (model)
            {
                case RoleBasic role:
                    AssertRoleIsSimilarToCreatedEvent(role, (RoleCreated)createdEvent);

                    return;
                case FunctionBasic func:
                    AssertFunctionIsSimilarToCreatedEvent(func, (FunctionCreated)createdEvent);

                    return;
                case GroupBasic group:
                    AssertGroupIsSimilarToCreatedEvent(group, (GroupCreated)createdEvent);

                    return;
                case UserBasic user:
                    AssertUserIsSimilarToCreatedEvent(user, (UserCreated)createdEvent);

                    return;
                case OrganizationBasic org:
                    AssertOrganizationIsSimilarToCreatedEvent(org, (OrganizationCreated)createdEvent);

                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model));
            }
        }

        public static void AssertOrganizationIsSimilarToCreatedEvent(
            OrganizationBasic organization,
            OrganizationCreated createdEvent)
        {
            organization.Should()
                .BeEquivalentTo(
                    createdEvent,
                    options => options
                        .Excluding(created => created.Type)
                        .Excluding(created => created.EventId)
                        .Excluding(created => created.MetaData)
                        .Excluding(info => info.Path == "Tags"));
        }

        public static void AssertUserIsSimilarToCreatedEvent(
            UserBasic group,
            UserCreated createdEvent)
        {
            group.Should()
                .BeEquivalentTo(
                    createdEvent,
                    options => options
                        .Excluding(created => created.Type)
                        .Excluding(created => created.EventId)
                        .Excluding(created => created.MetaData)
                        .Excluding(info => info.Path == "Tags"));
        }

        public static void AssertGroupIsSimilarToCreatedEvent(
            GroupBasic group,
            GroupCreated createdEvent)
        {
            group.Should()
                .BeEquivalentTo(
                    createdEvent,
                    options => options
                        .Excluding(created => created.Type)
                        .Excluding(created => created.EventId)
                        .Excluding(created => created.MetaData)
                        .Excluding(info => info.Path == "Tags"));
        }

        public static void AssertRoleIsSimilarToCreatedEvent(
            RoleBasic role,
            RoleCreated createdEvent)
        {
            role.Type.Should().Be(RoleType.Role);

            role.Should()
                .BeEquivalentTo(
                    createdEvent,
                    options => options
                        .Excluding(created => created.Type)
                        .Excluding(created => created.EventId)
                        .Excluding(created => created.MetaData)
                        .Excluding(info => info.Path == "Tags"));
        }

        public static void AssertFunctionIsSimilarToCreatedEvent(
            FunctionBasic function,
            FunctionCreated createdEvent)
        {
            function.Type.Should().Be(RoleType.Function);
            function.OrganizationId.Should().NotBeNullOrWhiteSpace();
            function.OrganizationId.Should().Be(createdEvent.Organization.Id);
            function.RoleId.Should().NotBeNullOrWhiteSpace();
            function.RoleId.Should().Be(createdEvent.Role.Id);

            function.Should()
                .BeEquivalentTo(
                    createdEvent,
                    options => options
                        .Excluding(created => created.Type)
                        .Excluding(created => created.EventId)
                        .Excluding(created => created.MetaData)
                        .Excluding(created => created.Tags)
                        .Excluding(info => info.Path == "Organization.ContainerType")
                        .Excluding(info => info.Path == "Role.ContainerType"));
        }
    }
}
