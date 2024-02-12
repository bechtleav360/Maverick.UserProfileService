using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class ProfileTagsAddEventHandlerTests
    {
        private readonly IMapper _mapper;

        public ProfileTagsAddEventHandlerTests()
        {
            _mapper = new Mapper(
                new MapperConfiguration(
                    opt =>
                    {
                        opt
                            .CreateMap<TagAssignment, FirstLevelProjectionTag>()
                            .ForMember(t => t.Id, s => s.MapFrom(t => t.TagId))
                            .ReverseMap();
                    }));
        }

        private ProfileTagsAddedEvent GenerateDefaultTagsAddedEvent(bool allIsInheritable = true)
        {
            var profile = new ProfileIdent("profile-group-1", ProfileKind.Group);
            List<TagAssignment> tagAssignments = MockDataGenerator.GenerateTagAssignments(2, allIsInheritable);

            ProfileTagsAddedEvent profileTagsAddedEvent =
                MockedSagaWorkerEventsBuilder.GenerateProfileTagsAddedEvent(profile, tagAssignments);

            return profileTagsAddedEvent;
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        [Theory]
        [InlineData(false, 0, 1)]
        [InlineData(false, 1, 1)]
        [InlineData(true, 0, 1)]
        [InlineData(true, 1, 2)]
        public async Task HandleEventAsync_Success(
            bool allIsInheritable,
            int childrenCount,
            int expectedEvents)
        {
            // Arrange
            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent(allIsInheritable);
            string profileId = profileTagsAddedEvent.Payload.Id;
            TagAssignment[] tags = profileTagsAddedEvent.Payload.Tags;

            var transaction = new MockDatabaseTransaction();
            

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);

            IList<IFirstLevelProjectionProfile> children = MockDataGenerator
                .GenerateFirstLevelProjectionGroup(childrenCount)
                .Cast<IFirstLevelProjectionProfile>()
                .ToList();

            repoMock.Setup(
                    s => s.GetAllChildrenAsync(
                        It.Is<ObjectIdent>(t => t.Id == profileId),
                        transaction,
                        CancellationToken.None))
                    .ReturnsAsync(
                        children.Select(
                                    profile => new FirstLevelRelationProfile(
                                        profile,
                                        FirstLevelMemberRelation.DirectMember))
                                .ToList());

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        new FirstLevelProjectionGroup
                        {
                            Id = profileId
                        });

            foreach (TagAssignment tag in tags)
            {
                var firstLevelTag = _mapper.Map<FirstLevelProjectionTag>(tag);

                repoMock.Setup(t => t.GetTagAsync(tag.TagId, transaction, CancellationToken.None))
                    .ReturnsAsync(firstLevelTag);

                repoMock.Setup(
                        t => t.AddTagToProfileAsync(
                            It.Is<FirstLevelProjectionTagAssignment>(t => t.TagId == tag.TagId),
                            profileId,
                            transaction,
                            CancellationToken.None))
                    .Returns(Task.CompletedTask);

                foreach (IFirstLevelProjectionProfile child in children)
                {
                    repoMock.Setup(
                            t => t.AddTagToProfileAsync(
                                It.Is<FirstLevelProjectionTagAssignment>(t => t.TagId == tag.TagId),
                                child.Id,
                                transaction,
                                CancellationToken.None))
                        .Returns(Task.CompletedTask);
                }
            }

            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act
            await handler.HandleEventAsync(
                profileTagsAddedEvent,
                profileTagsAddedEvent.GenerateEventHeader(20),
                CancellationToken.None);

            // Assert
            IReadOnlyDictionary<Guid, List<EventTuple>> sagas = sagaService.GetDictionary();
            KeyValuePair<Guid, List<EventTuple>> saga = Assert.Single(sagas);
            Assert.Equal(expectedEvents, saga.Value.Count);

            foreach (TagAssignment tag in tags)
            {
                var firstLevelTag = _mapper.Map<FirstLevelProjectionTag>(tag);

                repoMock.Setup(t => t.GetTagAsync(tag.TagId, transaction, CancellationToken.None))
                    .ReturnsAsync(firstLevelTag);

                repoMock.Verify(
                    t => t.AddTagToProfileAsync(
                        It.Is<FirstLevelProjectionTagAssignment>(t => t.TagId == tag.TagId),
                        profileId,
                        transaction,
                        CancellationToken.None),
                    Times.Once);

                if (allIsInheritable)
                {
                    foreach (IFirstLevelProjectionProfile child in children)
                    {
                        repoMock.Verify(
                            t => t.AddTagToProfileAsync(
                                It.Is<FirstLevelProjectionTagAssignment>(t => t.TagId == tag.TagId),
                                child.Id,
                                transaction,
                                CancellationToken.None),
                            Times.Once);
                    }
                }
            }
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_EventIsNull()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleEventAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TransactionIsNull()
        {
            // Arrange
            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent();

            var repoMock = new Mock<IFirstLevelProjectionRepository>();
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleEventAsync(
                    profileTagsAddedEvent,
                    profileTagsAddedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_PayloadIsNull()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent();
            profileTagsAddedEvent.Payload = null;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsAddedEvent,
                    profileTagsAddedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task HandleEventAsync_Should_Throw_Exception_If_ProfileIsNullOrWhitespace(string profileId)
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent();
            profileTagsAddedEvent.Payload.Id = profileId;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsAddedEvent,
                    profileTagsAddedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TagsAreNull()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent();
            profileTagsAddedEvent.Payload.Tags = null;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsAddedEvent,
                    profileTagsAddedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TagsAreEmpty()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            ProfileTagsAddedEvent profileTagsAddedEvent = GenerateDefaultTagsAddedEvent();
            profileTagsAddedEvent.Payload.Tags = Array.Empty<TagAssignment>();

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsAddedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsAddedEvent,
                    profileTagsAddedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }
    }
}
