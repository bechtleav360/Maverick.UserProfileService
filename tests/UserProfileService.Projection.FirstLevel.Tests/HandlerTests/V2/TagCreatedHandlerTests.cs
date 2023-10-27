using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Comparer;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class TagCreatedHandlerTests
    {
        private readonly TagCreatedEvent _createdTagEvent;
        private readonly FirstLevelProjectionTag _tag;

        public TagCreatedHandlerTests()
        {
            _tag = MockDataGenerator.GenerateFirstLevelTags().Single();
            _tag.Type = TagType.FunctionalAccessRights;
            _createdTagEvent = MockedSagaWorkerEventsBuilder.CreateTagCreatedEvent(_tag);
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_event()
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

            var sut = ActivatorUtilities.CreateInstance<TagCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_transaction()
        {
            //arrange
            var repoMock = new Mock<IFirstLevelProjectionRepository>();
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<TagCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _createdTagEvent,
                    _createdTagEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_work()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton<ISagaService>(sagaService);
                    s.AddSingleton(repositoryMock.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<TagCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdTagEvent,
                _createdTagEvent.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateTag(
                    It.Is(_tag, new FirstLevelTagComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));
        }
    }
}
