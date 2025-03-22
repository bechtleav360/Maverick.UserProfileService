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
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V3;
using UserProfileService.Projection.FirstLevel.UnitTests.Comparer;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using UserCreatedEventV3 = UserProfileService.Events.Implementation.V3.UserCreatedEvent;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V3
{
    public class UserCreatedHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly UserCreatedEventV3 _createdUserEventWithoutTags;
        private readonly UserCreatedEventV3 _createdUserEventWithTags;
        private readonly List<FirstLevelProjectionTag> _fistLevelProjectionsTags;
        private readonly List<TagAssignment> _tagsAssignments;
        private readonly FirstLevelProjectionUser _user;

        public UserCreatedHandlerTest()
        {
            _user = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _tagsAssignments = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true);

            _createdUserEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateUserCreatedEventV3(_user);

            _createdUserEventWithTags = MockedSagaWorkerEventsBuilder.CreateUserCreatedEventV3(_user, _tagsAssignments);

            _fistLevelProjectionsTags = _tagsAssignments.Select(
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

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

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

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _createdUserEventWithoutTags,
                    _createdUserEventWithoutTags.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_work_without_tags()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);

            repositoryMock.Setup(
                    repo => repo.CreateProfileAsync(
                        It.IsAny<FirstLevelProjectionUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.UserExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        CancellationToken.None))
                .Returns(Task.FromResult(false));

            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdUserEventWithoutTags,
                _createdUserEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(_user, new FirstLevelUserComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(1));

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Handler_should_throw_when_user_already_exist()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);

            repositoryMock.Setup(
                    repo => repo.CreateProfileAsync(
                        It.IsAny<FirstLevelProjectionUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.UserExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        CancellationToken.None))
                .Returns(Task.FromResult(true));

            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<AlreadyExistsException>(
                () => sut.HandleEventAsync(
                    _createdUserEventWithTags,
                    _createdUserEventWithTags.GenerateEventHeader(10),
                    CancellationToken.None));

            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(_user, new FirstLevelUserComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Never);

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
                    repo => repo.CreateProfileAsync(
                        It.IsAny<FirstLevelProjectionUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.UserExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        CancellationToken.None))
                .Returns(Task.FromResult(false));

            repositoryMock.Setup(
                    repo => repo.AddTagToProfileAsync(
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
                        _fistLevelProjectionsTags.First(t => t.Id == id));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdUserEventWithTags,
                _createdUserEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(_user, new FirstLevelUserComparer()),
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

            List<string> listOfFirstLevelTagIds = _fistLevelProjectionsTags.Select(tag => tag.Id).ToList();

            repositoryMock.Verify(
                repo => repo.AddTagToProfileAsync(
                    It.Is(
                        (FirstLevelProjectionTagAssignment tag) =>
                            CheckAndRemoveItem(listOfFirstLevelTagIds, tag.TagId)),
                    It.Is((string id) => id == _user.Id),
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
                    repo => repo.CreateProfileAsync(
                        It.IsAny<FirstLevelProjectionUser>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.UserExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        CancellationToken.None))
                .Returns(Task.FromResult(false));

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

            var sut = ActivatorUtilities.CreateInstance<UserCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => sut.HandleEventAsync(
                    _createdUserEventWithTags,
                    _createdUserEventWithTags.GenerateEventHeader(10),
                    CancellationToken.None));
        }
    }
}
