using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Handler;
using UserProfileService.Projection.SecondLevel.Assignments.Tests.Helpers;
using UserProfileService.Projection.SecondLevel.Assignments.Tests.Mocks;
using UserProfileService.Projection.SecondLevel.Assignments.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using Organization = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests.HandlerTests
{
    public class WasAssignedToFunctionHandlerTest
    {
        private const string RoleAName = "Lesen";
        private const string RoleBName = "Schreiben";
        private const string OrganizationAName = "999";
        private const string OrganizationBName = "1234";
        private readonly ObjectIdent _functionA = new ObjectIdent(
            "ECD168C9-C3AF-4F31-AA5D-4237A87B9C1C",
            ObjectType.Function);
        private readonly ObjectIdent _functionB = new ObjectIdent(
            "8925CA64-67C4-433B-BD68-B52DCD7046FE",
            ObjectType.Function);
        private readonly ObjectIdent _orgUnitA = new ObjectIdent(
            "84902B29-9532-4F2F-A9A1-6080D5717820",
            ObjectType.Organization);
        private readonly ObjectIdent _orgUnitB = new ObjectIdent(
            "97C0C267-DBDB-4415-B6BB-B8F84EF0CEE2",
            ObjectType.Organization);
        private readonly ObjectIdent _roleA = new ObjectIdent(
            "CFCCE725-6C70-4E5D-8A93-BD8352118301",
            ObjectType.Role);
        private readonly ObjectIdent _roleB = new ObjectIdent(
            "36E21DAD-C3B4-4B3D-B2FF-0960317C40C5",
            ObjectType.Role);

        private readonly ObjectIdent _user = new ObjectIdent(
            "777DD1DA-2727-46D5-990A-0DD0A0F4A8AC",
            ObjectType.User);

        private Mock<ISecondLevelAssignmentRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<ISecondLevelAssignmentRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            return mock;
        }

        private WasAssignedToFunction GetNewEvent(
            IServiceProvider serviceProvider,
            string childId,
            string parentId,
            ObjectIdent role,
            ObjectIdent organization,
            params RangeCondition[] assignments)
        {
            var lookup = new Dictionary<string, string>
            {
                { _roleA.Id, RoleAName },
                { _roleB.Id, RoleBName },
                { _orgUnitA.Id, OrganizationAName },
                { _orgUnitB.Id, OrganizationBName }
            };

            var eventRole = new Role
            {
                Id = role.Id,
                Name = lookup.GetValueOrDefault(role.Id, "Role")
            };

            var eventOrgUnit = new Organization
            {
                Id = organization.Id,
                Name = lookup.GetValueOrDefault(organization.Id, "OE")
            };

            WasAssignedToFunction newEvent = new WasAssignedToFunction
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                ProfileId = childId,
                Target = new Function
                {
                    Id = parentId,
                    Organization = eventOrgUnit,
                    Role = eventRole,
                    RoleId = role.Id,
                    OrganizationId = organization.Id
                },
                Conditions = assignments
            }.AddDefaultMetadata(serviceProvider);

            return newEvent;
        }

        private SecondLevelProjectionAssignmentsUser GetNewUser(string userId)
        {
            var user = new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = _roleB.Id,
                        Name = RoleBName,
                        ContainerType = ContainerType.Role
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = _orgUnitB.Id,
                        Name = OrganizationBName,
                        ContainerType = ContainerType.Organization
                    },
                    new SecondLevelAssignmentFunction
                    {
                        Id = _functionB.Id,
                        Name = $"{OrganizationBName} {RoleAName}",
                        RoleId = _roleB.Id,
                        OrganizationId = _orgUnitB.Id
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new List<Maverick.UserProfileService.Models.Models.RangeCondition>
                        {
                            new Maverick.UserProfileService.Models.Models.RangeCondition(
                                DateTime.MinValue,
                                DateTime.Today)
                        },
                        Parent = new ObjectIdent(
                            _functionB.Id,
                            ObjectType.Function),
                        Profile = new ObjectIdent(
                            userId,
                            ObjectType.User)
                    }
                }
            };

            user.ActiveMemberships = user.CalculateActiveMemberships().ToList();

            return user;
        }

        [Fact]
        public async Task Handle_event_should_add_new_function_with_containers()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            SecondLevelProjectionAssignmentsUser savedUser = null;

            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            repoMock.Setup(
                    r => r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .Callback<SecondLevelProjectionAssignmentsUser, IDatabaseTransaction, CancellationToken>(
                    (user, _, _) => savedUser = user);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToFunctionHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToFunction eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _functionA.Id,
                _roleA,
                _orgUnitA,
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.MaxValue
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                ct);

            // assert
            repoMock.Verify(
                r => r.SaveAssignmentUserAsync(
                    It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            repoMock.Verify(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            SecondLevelProjectionAssignmentsUser expectedUser = GetNewUser(_user.Id);

            expectedUser.Containers.Add(
                new SecondLevelAssignmentFunction
                {
                    Id = _functionA.Id,
                    Name = $"{OrganizationAName} {RoleAName}",
                    RoleId = _roleA.Id,
                    OrganizationId = _orgUnitA.Id
                });

            expectedUser.Containers.Add(
                new SecondLevelAssignmentContainer
                {
                    Id = _roleA.Id,
                    Name = RoleAName,
                    ContainerType = ContainerType.Role
                });

            expectedUser.Containers.Add(
                new SecondLevelAssignmentContainer
                {
                    Id = _orgUnitA.Id,
                    Name = OrganizationAName,
                    ContainerType = ContainerType.Organization
                });

            expectedUser.Assignments.Add(
                new SecondLevelProjectionAssignment
                {
                    Conditions = new List<Maverick.UserProfileService.Models.Models.RangeCondition>
                    {
                        new Maverick.UserProfileService.Models.Models.RangeCondition(
                            DateTime.Today,
                            DateTime.MaxValue)
                    },
                    Parent = _functionA,
                    Profile = _user
                });

            expectedUser.ActiveMemberships.Add(_functionA);

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_event_should_add_new_assignment()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            SecondLevelProjectionAssignmentsUser savedUser = null;

            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            repoMock.Setup(
                    r => r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .Callback<SecondLevelProjectionAssignmentsUser, IDatabaseTransaction, CancellationToken>(
                    (user, _, _) => savedUser = user);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToFunctionHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToFunction eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _functionB.Id,
                _roleB,
                _functionB,
                new RangeCondition
                {
                    Start = DateTime.Today,
                    End = DateTime.MaxValue
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                ct);

            // assert
            repoMock.Verify(
                r => r.SaveAssignmentUserAsync(
                    It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            repoMock.Verify(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)),
                Times.Once);

            SecondLevelProjectionAssignmentsUser expectedUser = GetNewUser(_user.Id);
            expectedUser.ActiveMemberships.Add(_functionB);

            SecondLevelProjectionAssignment assignment =
                expectedUser.Assignments.First(a => a.Parent.Id == _functionB.Id && a.Profile.Id == _user.Id);

            assignment.Conditions.Add(
                new Maverick.UserProfileService.Models.Models.RangeCondition(DateTime.Today, DateTime.MaxValue));

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_cancelled_event_should_throw_cancelled_exception()
        {
            // arrange
            var ct = new CancellationToken(true);
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToFunctionHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToFunction eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _functionA.Id,
                _roleA,
                _orgUnitB,
                new RangeCondition());

            // act & assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => sut.HandleEventAsync(
                    eventToHandle,
                    eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                    ct));
        }

        [Fact]
        public async Task Handle_event_for_group_should_not_call_repo()
        {
            // arrange
            var ct = new CancellationToken(false);
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToFunctionHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasAssignedToFunction eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _functionA.Id,
                _roleA,
                _orgUnitB,
                new RangeCondition());

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(
                        new ObjectIdent(
                            Guid.NewGuid().ToString("D"),
                            ObjectType.Function))),
                ct);

            // assert
            repoMock.Verify(
                r =>
                    r.GetAssignmentUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(
                r =>
                    r.SaveAssignmentUserAsync(
                        It.IsAny<SecondLevelProjectionAssignmentsUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(
                r =>
                    r.RemoveAssignmentUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
