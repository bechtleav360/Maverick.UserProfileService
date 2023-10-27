using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Extensions;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class TagDeletedEventHandlerTest
    {
        private const int NumberProfileTags = 30;
        private const int NumberRolesTags = 25;
        private const int NumberFunctionTags = 15;
        private readonly List<ObjectIdent> _allObjectIdents;
        private readonly List<IFirstLevelProjectionContainer> _containerTagIsAssignedTo;
        private readonly TagDeletedEvent _deletedEvent;
        private readonly FirstLevelProjectionTag _firstLevelTag;
        private readonly List<IFirstLevelProjectionProfile> _profilesTagIsAssignedTo;

        public TagDeletedEventHandlerTest()
        {
            _firstLevelTag = MockDataGenerator.GenerateFirstLevelTags().Single();

            _profilesTagIsAssignedTo = MockDataGenerator.GenerateFirstLevelProjectionUser(5)
                .ToFirstLevelProfileList()
                .Concat(
                    MockDataGenerator.GenerateFirstLevelProjectionGroup(10)
                        .ToFirstLevelProfileList())
                .Concat(
                    MockDataGenerator
                        .GenerateFirstLevelProjectionOrganizationInstances(15)
                        .ToFirstLevelProfileList())
                .ToList();

            _containerTagIsAssignedTo = MockDataGenerator
                .GenerateFirstLevelProjectionFunctionInstances(NumberFunctionTags)
                .ToFirstLevelContainerList()
                .Concat(
                    MockDataGenerator.GenerateFirstLevelRoles(25)
                        .ToFirstLevelContainerList())
                .ToList();

            _allObjectIdents = _profilesTagIsAssignedTo.Select(pr => pr.ToObjectIdent())
                .Concat(_containerTagIsAssignedTo.Select(con => con.ToObjectIdent()))
                .ToList();

            _deletedEvent = MockedSagaWorkerEventsBuilder.CreateTagDeletedEvent(_firstLevelTag);
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        private bool CheckAndRemoveItem(List<string> ids, string id)
        {
            lock (ids)
            {
                int position = ids.IndexOf(id);

                if (position == -1)
                {
                    return false;
                }

                ids.RemoveAt(position);

                return true;
            }
        }

        [Fact]
        public async Task Handler_should_work_with_all_entities()
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

            repoMock.Setup(
                    repo => repo.GetAssignedObjectsFromTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(_allObjectIdents);

            repoMock.Setup(
                    repo => repo.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(_firstLevelTag);

            repoMock.Setup(
                    repo => repo.DeleteTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

            var sut = ActivatorUtilities.CreateInstance<TagDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _deletedEvent,
                _deletedEvent.GenerateEventHeader(
                    10,
                    streamNameResolve.GetStreamName(new ObjectIdent(_firstLevelTag.Id, ObjectType.Tag))));

            repoMock.Verify(
                repo => repo.GetAssignedObjectsFromTagAsync(
                    It.Is((string id) => id == _firstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.Is((string id) => id == _firstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.DeleteTagAsync(
                    It.Is((string id) => id == _firstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Handler_should_work_with_compare_output_parameter()
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

            repoMock.Setup(
                    repo => repo.GetAssignedObjectsFromTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(InputSagaWorkerEventsOutputEventTuple.TagToObjectIdents);

            repoMock.Setup(
                    repo => repo.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(InputSagaWorkerEventsOutputEventTuple.FirstLevelTag);

            repoMock.Setup(
                    repo => repo.DeleteTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var streamNameResolve = services.GetRequiredService<IStreamNameResolver>();

            var sut = ActivatorUtilities.CreateInstance<TagDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                InputSagaWorkerEventsOutputEventTuple.TagDeletedEvent,
                InputSagaWorkerEventsOutputEventTuple.TagDeletedEvent.GenerateEventHeader(
                    10,
                    streamNameResolve.GetStreamName(new ObjectIdent(_firstLevelTag.Id, ObjectType.Tag))));

            repoMock.Verify(
                repo => repo.GetAssignedObjectsFromTagAsync(
                    It.Is((string id) => id == InputSagaWorkerEventsOutputEventTuple.FirstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.Is((string id) => id == InputSagaWorkerEventsOutputEventTuple.FirstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.DeleteTagAsync(
                    It.Is((string id) => id == InputSagaWorkerEventsOutputEventTuple.FirstLevelTag.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            List<EventTuple> secondLevelEvents = sagaService.GetDictionary().First().Value;

            InputSagaWorkerEventsOutputEventTuple.ResolvedTagDeletedTuple.Should()
                .BeEquivalentTo(
                    secondLevelEvents,
                    opt => opt.Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .Excluding(p => p.Event.EventId)
                        .RespectingRuntimeTypes());
        }
    }
}
