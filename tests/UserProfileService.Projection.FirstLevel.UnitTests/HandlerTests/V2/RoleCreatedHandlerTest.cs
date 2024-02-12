using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Comparer;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class RoleCreatedHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly RoleCreatedEvent _createdRoleEventWithoutTags;
        private readonly RoleCreatedEvent _createdRoleEventWithTags;
        private readonly List<FirstLevelProjectionTag> _firstLevelProjectionTags;
        private readonly FirstLevelProjectionRole _role;
        private readonly List<TagAssignment> _tagsAssignments;

        public RoleCreatedHandlerTest()
        {
            _role = MockDataGenerator.GenerateFirstLevelRoles().Single();
            _tagsAssignments = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true);

            _createdRoleEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateRoleCreatedEvent(_role);

            _createdRoleEventWithTags =
                MockedSagaWorkerEventsBuilder.CreateRoleCreatedEvent(_role, _tagsAssignments);

            _firstLevelProjectionTags = _tagsAssignments.Select(
                    tag => new FirstLevelProjectionTag
                    {
                        Id = tag.TagId,
                        Type = TagType.Custom,
                        Name = string.Empty
                    })
                .ToList();
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>();

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

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedFirstLevelEventHandler>(services);

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

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _createdRoleEventWithTags,
                    _createdRoleEventWithTags.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_work_without_tags()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdRoleEventWithoutTags,
                _createdRoleEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateRoleAsync(
                    It.Is(_role, new FirstLevelRoleComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Handler_work_with_tagsAssignments()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    repo => repo.AddTagToRoleAsync(
                        It.IsAny<FirstLevelProjectionTagAssignment>(),
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    opt => opt.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction trans, CancellationToken cancellationToken) =>
                        _firstLevelProjectionTags.First(t => t.Id == id));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdRoleEventWithTags,
                _createdRoleEventWithTags.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateRoleAsync(
                    It.Is(_role, new FirstLevelRoleComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            List<string> ids = _tagsAssignments.Select(t => t.TagId).ToList();

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.Is((string id) => CheckAndRemoveItem(ids, id)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(ids);

            List<string> listOfFirstLevelTagIds = _firstLevelProjectionTags.Select(tag => tag.Id).ToList();

            repositoryMock.Verify(
                repo => repo.AddTagToRoleAsync(
                    It.Is(
                        (FirstLevelProjectionTagAssignment tag) =>
                            CheckAndRemoveItem(listOfFirstLevelTagIds, tag.TagId)),
                    It.Is((string id) => id == _role.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(listOfFirstLevelTagIds);
        }

        [Fact]
        public async Task Handler_work_with_tagsAssignments_throw_exception()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    opt => opt.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction trans, CancellationToken cancellationToken) =>
                        throw new InstanceNotFoundException());

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.HandleEventAsync(
                    _createdRoleEventWithTags,
                    _createdRoleEventWithTags.GenerateEventHeader(10),
                    CancellationToken.None));
        }
    }
}
