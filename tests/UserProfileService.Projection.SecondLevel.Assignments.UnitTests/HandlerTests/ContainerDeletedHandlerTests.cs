using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Handler;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Mocks;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests.HandlerTests
{
    public class ContainerDeletedHandlerTests
    {
        private readonly ObjectIdent _user = new ObjectIdent("777DD1DA-2727-46D5-990A-0DD0A0F4A8AC", ObjectType.User);

        private static Mock<ISecondLevelAssignmentRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<ISecondLevelAssignmentRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            return mock;
        }

        private static SecondLevelProjectionAssignmentsUser GetNewUser(string userId)
        {
            return new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = userId,
                Containers = new List<ISecondLevelAssignmentContainer>
                {
                    new SecondLevelAssignmentContainer
                    {
                        Id = "group-A",
                        Name = "Gruppe A",
                        ContainerType = ContainerType.Group
                    },
                    new SecondLevelAssignmentContainer
                    {
                        Id = "group-b",
                        Name = "Gruppe B",
                        ContainerType = ContainerType.Group
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent("group-a", ObjectType.Group),
                        Profile = new ObjectIdent("group-b", ObjectType.Group)
                    },
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition() },
                        Parent = new ObjectIdent("group-b", ObjectType.Group),
                        Profile = new ObjectIdent(userId, ObjectType.User)
                    }
                }
            };
        }

        private static ContainerDeleted GetNewEvent(
            IServiceProvider serviceProvider,
            string containerId,
            string memberId,
            Action<ContainerDeleted> postModifications = null,
            ContainerType targetType = ContainerType.Group)
        {
            ContainerDeleted containerDeleted = new ContainerDeleted
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                ContainerId = containerId,
                ContainerType = targetType,
                MemberId = memberId
            }.AddDefaultMetadata(serviceProvider);

            postModifications?.Invoke(containerDeleted);

            return containerDeleted;
        }

        [Fact]
        public async Task Handle_event_should_work()
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

            var sut = ActivatorUtilities.CreateInstance<ContainerDeletedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            ContainerDeleted eventToHandle = GetNewEvent(services, "group-b", _user.Id);

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
                    ItShould.BeEquivalentTo(ct)));

            savedUser.Should()
                .BeEquivalentTo(
                    new SecondLevelProjectionAssignmentsUser
                    {
                        Assignments = Array.Empty<SecondLevelProjectionAssignment>(),
                        Containers = Array.Empty<ISecondLevelAssignmentContainer>(),
                        ActiveMemberships = Array.Empty<ObjectIdent>(),
                        ProfileId = _user.Id
                    });
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

            var sut = ActivatorUtilities.CreateInstance<ContainerDeletedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            ContainerDeleted eventToHandle = GetNewEvent(
                services,
                "group-a",
                "group-b");

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
            var ct = new CancellationToken(true);
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                r => r.GetAssignmentUserAsync(
                    It.Is<string>(s => s == _user.Id),
                    ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                    ItShould.BeEquivalentTo(ct)));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<ContainerDeletedHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            ContainerDeleted eventToHandle = GetNewEvent(
                services,
                "group-a",
                "group-b");

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
        }
    }
}
