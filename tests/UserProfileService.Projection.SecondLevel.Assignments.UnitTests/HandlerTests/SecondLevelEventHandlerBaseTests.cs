using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests.HandlerTests
{
    public class SecondLevelEventHandlerBaseTests
    {
        private readonly ObjectIdent _user;

        public SecondLevelEventHandlerBaseTests()
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

        private static MockUpsEvent GetNewEvent(
            IServiceProvider serviceProvider,
            Action<MockUpsEvent> postModifications = null,
            ObjectType targetType = ObjectType.Group)
        {
            MockUpsEvent newEvent = new MockUpsEvent().AddDefaultMetadata(serviceProvider);

            postModifications?.Invoke(newEvent);

            return newEvent;
        }

        [Fact]
        public async Task Handle_event_should_throw_null_events()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<MockSecondLevelEventHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            MockUpsEvent eventToHandle = GetNewEvent(services);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    null,
                    eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                    ct));
        }

        [Fact]
        public async Task Handle_event_should_throw_null_event_header()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<MockSecondLevelEventHandler>(services);

            MockUpsEvent eventToHandle = GetNewEvent(services);

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    eventToHandle,
                    null,
                    ct));
        }

        [Fact]
        public async Task Handle_event_should_throw_on_invalid_stream_name()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<MockSecondLevelEventHandler>(services);

            MockUpsEvent eventToHandle = GetNewEvent(services);

            // act & assert
            await Assert.ThrowsAsync<InvalidHeaderException>(
                () => sut.HandleEventAsync(
                    eventToHandle,
                    new StreamedEventHeader
                    {
                        EventStreamId = string.Empty
                    },
                    ct));
        }

        [Fact]
        public async Task Handle_event_should_rethrow_thrown_exceptions()
        {
            // arrange
            var ct = new CancellationToken();
            var transaction = new MockDatabaseTransaction();
            Mock<ISecondLevelAssignmentRepository> repoMock = GetRepository(transaction);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s.AddSingleton(repoMock.Object));

            var sut = ActivatorUtilities.CreateInstance<MockSecondLevelEventHandler>(services);
            var streamNameResolver = services.GetRequiredService<IStreamNameResolver>();

            MockUpsEvent eventToHandle = GetNewEvent(services);

            // act & assert
            await Assert.ThrowsAsync<NotValidException>(
                () => sut.HandleEventAsync(
                    eventToHandle,
                    eventToHandle.GenerateEventHeader(42, streamNameResolver.GetStreamName(_user)),
                    ct));
        }
    }
}
