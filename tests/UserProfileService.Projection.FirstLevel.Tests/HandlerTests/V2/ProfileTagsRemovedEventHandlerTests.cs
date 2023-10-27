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
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class ProfileTagsRemovedEventHandlerTests
    {
        private readonly IMapper _mapper;

        public ProfileTagsRemovedEventHandlerTests()
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

        private (ProfileTagsRemovedEvent removedEvent, List<TagAssignment> tagAssignments)
            GenerateDefaultTagsRemovedEvent(bool allIsInheritable = true)
        {
            var profile = new ProfileIdent("profile-group-1", ProfileKind.Group);

            List<TagAssignment> tagAssignments = MockDataGenerator
                .GenerateTagAssignments(2, allIsInheritable);

            ProfileTagsRemovedEvent profileTagsRemovedEvent =
                MockedSagaWorkerEventsBuilder.GenerateProfileTagsRemovedEvent(
                    profile,
                    tagAssignments.Select(t => t.TagId).ToList());

            return (profileTagsRemovedEvent, tagAssignments);
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
            (ProfileTagsRemovedEvent profileTagsRemovedEvent, List<TagAssignment> tagAssignments) =
                GenerateDefaultTagsRemovedEvent(allIsInheritable);

            string profileId = profileTagsRemovedEvent.Payload.ResourceId;
            string[] tags = profileTagsRemovedEvent.Payload.Tags;

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
                                .ToList);

            List<FirstLevelProjectionTagAssignment> firstLevelProjectionTagAssignments = tagAssignments.Select(
                    t => new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = t.IsInheritable,
                        TagId = t.TagId
                    })
                .ToList();

            repoMock.Setup(
                    s => s.GetTagsAssignmentsFromProfileAsync(tags, profileId, transaction, CancellationToken.None))
                .ReturnsAsync(firstLevelProjectionTagAssignments);

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

            foreach (string tagId in tags)
            {
                var firstLevelTag = new FirstLevelProjectionTag
                {
                    Id = tagId,
                    Name = tagId,
                    Type = TagType.Custom
                };

                repoMock.Setup(t => t.GetTagAsync(tagId, transaction, CancellationToken.None))
                    .ReturnsAsync(firstLevelTag);

                repoMock.Setup(
                        t => t.RemoveTagFromProfileAsync(
                            tagId,
                            profileId,
                            transaction,
                            CancellationToken.None))
                    .Returns(Task.CompletedTask);

                foreach (IFirstLevelProjectionProfile child in children)
                {
                    repoMock.Setup(
                            t => t.RemoveTagFromProfileAsync(
                                tagId,
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

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act
            await handler.HandleEventAsync(
                profileTagsRemovedEvent,
                profileTagsRemovedEvent.GenerateEventHeader(20),
                CancellationToken.None);

            // Assert
            IReadOnlyDictionary<Guid, List<EventTuple>> sagas = sagaService.GetDictionary();
            KeyValuePair<Guid, List<EventTuple>> saga = Assert.Single(sagas);
            Assert.Equal(expectedEvents, saga.Value.Count);

            foreach (string tagId in tags)
            {
                var firstLevelTag = new FirstLevelProjectionTag
                {
                    Id = tagId,
                    Type = TagType.Custom,
                    Name = "Tag 1"
                };

                repoMock.Setup(t => t.GetTagAsync(tagId, transaction, CancellationToken.None))
                    .ReturnsAsync(firstLevelTag);

                repoMock.Verify(
                    t => t.RemoveTagFromProfileAsync(
                        tagId,
                        profileId,
                        transaction,
                        CancellationToken.None),
                    Times.Once);

                if (allIsInheritable)
                {
                    foreach (IFirstLevelProjectionProfile child in children)
                    {
                        repoMock.Verify(
                            t => t.RemoveTagFromProfileAsync(
                                tagId,
                                child.Id,
                                transaction,
                                CancellationToken.None),
                            Times.Never);
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

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleEventAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TransactionIsNull()
        {
            // Arrange
            (ProfileTagsRemovedEvent profileTagsRemovedEvent, _) = GenerateDefaultTagsRemovedEvent();

            var repoMock = new Mock<IFirstLevelProjectionRepository>();
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => handler.HandleEventAsync(
                    profileTagsRemovedEvent,
                    profileTagsRemovedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_PayloadIsNull()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            (ProfileTagsRemovedEvent profileTagsRemovedEvent, _) = GenerateDefaultTagsRemovedEvent();
            profileTagsRemovedEvent.Payload = null;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsRemovedEvent,
                    profileTagsRemovedEvent.GenerateEventHeader(10),
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

            (ProfileTagsRemovedEvent profileTagsRemovedEvent, _) = GenerateDefaultTagsRemovedEvent();
            profileTagsRemovedEvent.Payload.ResourceId = profileId;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsRemovedEvent,
                    profileTagsRemovedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TagsAreNull()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            (ProfileTagsRemovedEvent profileTagsRemovedEvent, _) = GenerateDefaultTagsRemovedEvent();
            profileTagsRemovedEvent.Payload.Tags = null;

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsRemovedEvent,
                    profileTagsRemovedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task HandleEventAsync_Should_Throw_Exception_If_TagsAreEmpty()
        {
            // Arrange
            var transaction = new MockDatabaseTransaction();

            (ProfileTagsRemovedEvent profileTagsRemovedEvent, _) = GenerateDefaultTagsRemovedEvent();
            profileTagsRemovedEvent.Payload.Tags = Array.Empty<string>();

            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var handler = ActivatorUtilities.CreateInstance<ProfileTagsRemovedFirstLevelEventHandler>(services);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleEventAsync(
                    profileTagsRemovedEvent,
                    profileTagsRemovedEvent.GenerateEventHeader(10),
                    CancellationToken.None));
        }
    }
}
