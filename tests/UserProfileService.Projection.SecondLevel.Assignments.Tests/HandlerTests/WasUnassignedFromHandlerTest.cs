using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
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
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests.HandlerTests
{
    public class WasUnassignedFromHandlerTest
    {
        private readonly ObjectIdent _function = new ObjectIdent(
            "8925CA64-67C4-433B-BD68-B52DCD7046FE",
            ObjectType.Function);
        private readonly ObjectIdent _group = new ObjectIdent(
            "55175384-9B20-4927-A1E5-7F11B258C5A7",
            ObjectType.Group);
        private readonly ObjectIdent _orgUnit = new ObjectIdent(
            "ECD168C9-C3AF-4F31-AA5D-4237A87B9C1C",
            ObjectType.Organization);
        private readonly ObjectIdent _role = new ObjectIdent(
            "478C8876-2E78-494E-9FE0-DC90434447D7",
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

        private static WasUnassignedFrom GetNewEvent(
            IServiceProvider serviceProvider,
            string childId,
            string parentId,
            ContainerType parentType,
            params RangeCondition[] assignments)
        {
            WasUnassignedFrom newEvent = new WasUnassignedFrom
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                ChildId = childId,
                ParentId = parentId,
                ParentType = parentType,
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
                        Id = _group.Id,
                        Name = "Gruppe A",
                        ContainerType = ContainerType.Group
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = _role.Id,
                        Name = "Lesen",
                        ContainerType = ContainerType.Role
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = _orgUnit.Id,
                        Name = "999",
                        ContainerType = ContainerType.Organization
                    },
                    new SecondLevelAssignmentFunction
                    {
                        Id = _function.Id,
                        Name = "999 Lesen",
                        OrganizationId = _orgUnit.Id,
                        RoleId = _role.Id
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new Maverick.UserProfileService.Models.Models.RangeCondition() },
                        Parent = new ObjectIdent(
                            _group.Id,
                            ObjectType.Group),
                        Profile = new ObjectIdent(
                            userId,
                            ObjectType.User)
                    },
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[]
                        {
                            new Maverick.UserProfileService.Models.Models.RangeCondition(
                                DateTime.MinValue,
                                DateTime.Today),
                            new Maverick.UserProfileService.Models.Models.RangeCondition(
                                DateTime.UnixEpoch,
                                DateTime.MaxValue)
                        },
                        Parent = new ObjectIdent(
                            _function.Id,
                            ObjectType.Function),
                        Profile = new ObjectIdent(
                            _group.Id,
                            ObjectType.Group)
                    }
                }
            };

            user.ActiveMemberships = user.CalculateActiveMemberships().ToList();

            return user;
        }

        [Fact]
        public async Task Handle_event_should_remove_single_condition()
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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _group.Id,
                _function.Id,
                ContainerType.Function,
                new RangeCondition
                {
                    Start = DateTime.MinValue,
                    End = DateTime.Today
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

            SecondLevelProjectionAssignment assignment =
                expectedUser.Assignments.First(a => a.Parent.Id == _function.Id && a.Profile.Id == _group.Id);

            assignment.Conditions = new List<Maverick.UserProfileService.Models.Models.RangeCondition>
            {
                new Maverick.UserProfileService.Models.Models.RangeCondition(DateTime.UnixEpoch, DateTime.MaxValue)
            };

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_event_should_remove_complete_assignment()
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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _group.Id,
                ContainerType.Group,
                new RangeCondition());

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
            expectedUser.Containers.Clear();
            expectedUser.Assignments.Clear();
            expectedUser.ActiveMemberships.Clear();

            savedUser.Should()
                .BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Handle_event_should_remove_complete_function()
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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _group.Id,
                _function.Id,
                ContainerType.Function,
                new RangeCondition
                {
                    Start = DateTime.MinValue,
                    End = DateTime.Today
                },
                new RangeCondition
                {
                    Start = DateTime.UnixEpoch,
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

            expectedUser.Containers =
                expectedUser.Containers.Where(c => c.ContainerType == ContainerType.Group).ToList();

            expectedUser.Assignments = expectedUser.Assignments.Where(a => a.Parent.Id == _group.Id).ToList();

            expectedUser.ActiveMemberships = new List<ObjectIdent>
            {
                _group
            };

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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _group.Id,
                ContainerType.Function,
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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _user.Id,
                _group.Id,
                ContainerType.Function,
                new RangeCondition());

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(
                        new ObjectIdent(
                            Guid.NewGuid().ToString("D"),
                            ObjectType.Group))),
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

        [Fact]
        public async Task Handle_event_with_unknown_assignment_should_not_save_in_repo()
        {
            // arrange
            var ct = new CancellationToken(false);
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

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            WasUnassignedFrom eventToHandle = GetNewEvent(
                services,
                _user.Id,
                "test-id",
                ContainerType.Function,
                new RangeCondition
                {
                    Start = DateTime.MinValue,
                    End = DateTime.Today
                });

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(_user)),
                ct);

            // assert
            repoMock.Verify(
                r =>
                    r.GetAssignmentUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()),
                Times.Once);

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
