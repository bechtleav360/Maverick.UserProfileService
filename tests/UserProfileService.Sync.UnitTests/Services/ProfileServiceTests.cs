using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Stores;
using UserProfileService.Sync.Projection.Services;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Services
{
    public class ProfileServiceTests
    {
        [Theory]
        [AutoData]
        public async Task GetUserAsync_should_work(UserSync user)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(p => p.GetProfileAsync<UserSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(user);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            UserSync repoUser = await profileService.GetProfileAsync<UserSync>(user.Id);

            //Assert
            profileStore.Verify(
                p => p.GetProfileAsync<UserSync>(It.Is<string>(i => i == user.Id), It.IsAny<CancellationToken>()),
                Times.Once);

            repoUser.Should().BeEquivalentTo(user);
        }

        [Theory]
        [AutoData]
        public async Task CreateUserAsync_should_work(UserSync user)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(p => p.CreateProfileAsync(It.IsAny<UserSync>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(user);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            UserSync repoUser = await profileService.CreateProfileAsync(user);

            //Assert
            profileStore.Verify(
                p => p.CreateProfileAsync(
                    It.Is<UserSync>(i => i.Id == user.Id && i.Name == user.Name && i.DisplayName == user.DisplayName),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoUser.Should().BeEquivalentTo(user);
        }

        [Theory]
        [AutoData]
        public async Task UpdateUserAsync(string id, UserSync user)
        {
            user.Id = id;

            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(
                            p => p.UpdateProfileAsync(
                                It.IsAny<UserSync>(),
                                It.IsAny<CancellationToken>()))
                        .ReturnsAsync(user);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            UserSync repoUser = await profileService.UpdateProfileAsync(user);

            //Assert
            profileStore.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<UserSync>(i => i.Id == user.Id && i.Name == user.Name && i.DisplayName == user.DisplayName),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoUser.Should().BeEquivalentTo(user);
        }

        [Theory]
        [AutoData]
        public async Task DeleteUserAsync(UserSync user)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            await profileService.DeleteProfileAsync<UserSync>(user.Id);

            //Assert
            profileStore.Verify(
                p => p.DeleteProfileAsync<UserSync>(It.Is<string>(i => i == user.Id), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetGroupAsync_should_work(GroupSync group)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(p => p.GetProfileAsync<GroupSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(group);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            GroupSync repoUser = await profileService.GetProfileAsync<GroupSync>(group.Id);

            //Assert
            profileStore.Verify(
                p => p.GetProfileAsync<GroupSync>(It.Is<string>(i => i == group.Id), It.IsAny<CancellationToken>()),
                Times.Once);

            repoUser.Should().BeEquivalentTo(group);
        }

        [Theory]
        [AutoData]
        public async Task CreateGroupAsync(GroupSync group)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(p => p.CreateProfileAsync(It.IsAny<GroupSync>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(group);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            GroupSync repoGroup = await profileService.CreateProfileAsync<GroupSync>(group);

            //Assert
            profileStore.Verify(
                p => p.CreateProfileAsync(
                    It.Is<GroupSync>(
                        i => i.Id == group.Id && i.Name == group.Name && i.DisplayName == group.DisplayName),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoGroup.Should().BeEquivalentTo(group);
        }

        [Theory]
        [AutoData]
        public async Task UpdateGroupAsync(string id, GroupSync group)
        {
            group.Id = id;

            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            profileStore.Setup(
                            p => p.UpdateProfileAsync(
                                It.IsAny<GroupSync>(),
                                It.IsAny<CancellationToken>()))
                        .ReturnsAsync(group);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            GroupSync repoUser = await profileService.UpdateProfileAsync(group);

            //Assert
            profileStore.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<GroupSync>(
                        i => i.Id == group.Id && i.Name == group.Name && i.DisplayName == group.DisplayName),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoUser.Should().BeEquivalentTo(group);
        }

        [Theory]
        [AutoData]
        public async Task DeleteGroupAsync(GroupSync group)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            await profileService.DeleteProfileAsync<GroupSync>(group.Id);

            //Assert
            profileStore.Verify(
                p => p.DeleteProfileAsync<GroupSync>(It.Is<string>(i => i == group.Id), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task TrySaveProjectionStateAsync_should_work(
            ProjectionState projectionState
        )
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            await profileService.TrySaveProjectionStateAsync(projectionState);

            //Assert
            projectionStateRepository.Verify(
                p => p.SaveProjectionStateAsync(
                    It.Is<ProjectionState>(
                        i => i.StreamName == projectionState.StreamName
                            && i.ErrorOccurred == projectionState.ErrorOccurred
                            && i.EventId == projectionState.EventId
                            && i.ProcessedOn == projectionState.ProcessedOn
                            && i.EventNumberVersion == projectionState.EventNumberVersion
                            && i.ProcessingStartedAt == projectionState.ProcessingStartedAt
                            && i.EventName == projectionState.EventName),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetLatestProjectedEventIdsAsync_should_work(Dictionary<string, ulong> state)
        {
            //Arrange
            ILogger<ProfileService> logger = new LoggerFactory().CreateLogger<ProfileService>();
            var profileStore = new Mock<IEntityStore>();
            var projectionStateRepository = new Mock<IProjectionStateRepository>();

            projectionStateRepository.Setup(p => p.GetLatestProjectedEventIdsAsync(It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(state);

            var profileService = new ProfileService(logger, profileStore.Object, projectionStateRepository.Object);

            //Act
            Dictionary<string, ulong> result = await profileService.GetLatestProjectedEventIdsAsync();
            result.Should().BeEquivalentTo(state);
        }
    }
}
