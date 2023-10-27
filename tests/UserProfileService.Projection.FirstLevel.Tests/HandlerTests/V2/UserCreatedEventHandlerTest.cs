using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Comparer;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;
using UserCreatedEventV3 = UserProfileService.Events.Implementation.V3.UserCreatedEvent;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class UserCreatedEventHandlerTest
    {
        public static FirstLevelProjectionUser User;
        public static UserCreatedEvent CreatedEvent;

        public UserCreatedEventHandlerTest()
        {
            User = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();

            CreatedEvent =
                MockedSagaWorkerEventsBuilder.CreateUserCreatedEvent(User);
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        public static IEnumerable<object[]> ExceptionData = new List<object[]>
                                                            {
                                                                new object[]
                                                                {
                                                                    CreatedEvent,
                                                                    null
                                                                },
                                                                new object[]
                                                                {
                                                                    null,
                                                                    new StreamedEventHeader()
                                                                }
                                                            };

        [Theory]
        [MemberData(nameof(ExceptionData))]
        public async Task Handler_should_fail_because_of_null_event(
            IUserProfileServiceEvent eventObject,
            StreamedEventHeader eventHeader)
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(eventObject, eventHeader, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_work()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repoMock.Setup(
                        repo => repo.CreateProfileAsync(
                            It.IsAny<FirstLevelProjectionUser>(),
                            It.IsAny<IDatabaseTransaction>(),
                            CancellationToken.None))
                    .Returns(Task.CompletedTask);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(CreatedEvent, CreatedEvent.GenerateEventHeader(20), CancellationToken.None);

            // If we get to this function, we already called the V3CreatedEventHandler
            repoMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(User, new FirstLevelUserComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));
        }
    }
}
