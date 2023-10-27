using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
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
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests.HandlerTests
{
    public class AssignmentConditionTriggeredHandlerTests
    {
        private readonly ObjectIdent _user;

        public AssignmentConditionTriggeredHandlerTests()
        {
            _user = new ObjectIdent(
                "777DD1DA-2727-46D5-990A-0DD0A0F4A8AC",
                Maverick.UserProfileService.Models.EnumModels.ObjectType.User);
        }

        private Mock<ISecondLevelAssignmentRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<ISecondLevelAssignmentRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            return mock;
        }

        private static SecondLevelProjectionAssignmentsUser GetNewUser(string userId, DateTime from, DateTime to)
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
                    }
                },
                Assignments = new List<SecondLevelProjectionAssignment>
                {
                    new SecondLevelProjectionAssignment
                    {
                        Conditions = new[] { new RangeCondition(from, to) },
                        Parent = new ObjectIdent(
                            "group-a",
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.Group),
                        Profile = new ObjectIdent(
                            userId,
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.User)
                    }
                }
            };
        }

        private static AssignmentConditionTriggered GetNewEvent(
            IServiceProvider serviceProvider,
            Action<AssignmentConditionTriggered> postModifications = null,
            ObjectType targetType = ObjectType.Group)
        {
            AssignmentConditionTriggered newEvent = new AssignmentConditionTriggered
            {
                EventId = "03d6a0f24f3340b984e63b0dceb69afb",
                ProfileId = "7F100468-690F-4BBA-B7F9-D61B97AB13EF",
                TargetId = "86728907-4300-414e-8d5f-2c64ff211d6d",
                TargetObjectType = targetType,
                IsActive = true
            }.AddDefaultMetadata(serviceProvider);

            postModifications?.Invoke(newEvent);

            return newEvent;
        }

        [Fact]
        public async Task Handle_event_should_work()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            repoMock.Setup(
                    r => r.GetAssignmentUserAsync(
                        It.Is<string>(s => s == _user.Id),
                        ItShould.BeEquivalentTo<IDatabaseTransaction>(transaction),
                        ItShould.BeEquivalentTo(ct)))
                .ReturnsAsync(GetNewUser(_user.Id, DateTime.MinValue, DateTime.MaxValue));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            AssignmentConditionTriggered eventToHandle = GetNewEvent(services);

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
                .ReturnsAsync(GetNewUser(_user.Id, DateTime.MinValue, DateTime.MaxValue));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            AssignmentConditionTriggered eventToHandle = GetNewEvent(services);

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

            var sut = ActivatorUtilities.CreateInstance<AssignmentConditionTriggeredHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            AssignmentConditionTriggered eventToHandle = GetNewEvent(services);

            // act
            await sut.HandleEventAsync(
                eventToHandle,
                eventToHandle.GenerateEventHeader(
                    42,
                    streamNameResolver.GetStreamName(
                        new ObjectIdent(
                            Guid.NewGuid().ToString("D"),
                            Maverick.UserProfileService.Models.EnumModels.ObjectType.Group))),
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
