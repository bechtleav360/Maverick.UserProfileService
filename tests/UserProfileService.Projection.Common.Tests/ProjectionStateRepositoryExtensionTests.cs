using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Exceptions;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Projection.Common.Tests.Helpers;
using Xunit;

namespace UserProfileService.Projection.Common.Tests
{
    public class ProjectionStateRepositoryExtensionTests
    {
        private static ILogger GetLogger(CountingHelper counter = null)
        {
            return counter != null
                ? LoggerFactory.Create(b => b.AddSimpleLogMessageCheckLogger((_, ll) => counter.IncrementCounter(ll)))
                    .CreateLogger<IProjectionStateRepository>()
                : LoggerFactory.Create(b => b.AddSimpleLogMessageCheckLogger(true))
                    .CreateLogger<IProjectionStateRepository>();
        }

        private static ProjectionState GetNewState()
        {
            return new ProjectionState
            {
                EventId = "115A41B1-97EB-4C01-8441-D0ECF23552D0",
                EventName = "UserCreated",
                EventNumberVersion = 34012,
                ProcessedOn = DateTimeOffset.UtcNow,
                StreamName = "tests_user_123"
            };
        }

        [Fact]
        public async Task Try_to_save_projection_state_without_any_errors_should_work()
        {
            // arrange
            IDatabaseTransaction transaction = new MockDatabaseTransaction();
            var repoMock = new Mock<IProjectionStateRepository>();
            var exceptionCounter = new CountingHelper();
            ILogger logger = GetLogger(exceptionCounter);
            IProjectionStateRepository sut = repoMock.Object;
            ProjectionState state = GetNewState();

            // act
            bool success = await sut.TrySaveProjectionStateAsync(
                state,
                transaction,
                logger);

            // assert
            Assert.True(success);
            Assert.Equal(0, exceptionCounter.CurrentCount);

            repoMock.Verify(
                r => r.SaveProjectionStateAsync(
                    ItShould.BeEquivalentTo(state),
                    It.Is<IDatabaseTransaction>(t => t == transaction),
                    It.Is<CancellationToken>(token => token == CancellationToken.None)),
                Times.Exactly(1));
        }

        [Fact]
        public async Task Try_to_save_projection_state_with_one_time_error_should_work()
        {
            // arrange
            IDatabaseTransaction transaction = new MockDatabaseTransaction();
            ProjectionState state = GetNewState();
            var repoMock = new Mock<IProjectionStateRepository>();

            repoMock.SetupSequence(
                    r => r.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProjectionRepositoryException("No way!"))
                .Returns(Task.CompletedTask);

            var exceptionCounter = new CountingHelper();
            ILogger logger = GetLogger(exceptionCounter);
            IProjectionStateRepository sut = repoMock.Object;

            // act
            bool success = await sut.TrySaveProjectionStateAsync(
                state,
                transaction,
                logger);

            // assert
            Assert.True(success);
            Assert.Equal(0, exceptionCounter.CurrentCount);

            repoMock.Verify(
                r => r.SaveProjectionStateAsync(
                    ItShould.BeEquivalentTo(state),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Try_to_save_projection_state_with_always_occurring_error_should_work()
        {
            // arrange
            IDatabaseTransaction transaction = new MockDatabaseTransaction();
            ProjectionState state = GetNewState();
            var repoMock = new Mock<IProjectionStateRepository>();

            repoMock.Setup(
                    r => r.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProjectionRepositoryException("No way!"));

            var exceptionCounter = new CountingHelper();
            ILogger logger = GetLogger(exceptionCounter);
            IProjectionStateRepository sut = repoMock.Object;

            // act
            bool success = await sut.TrySaveProjectionStateAsync(
                state,
                transaction,
                logger);

            // assert
            Assert.False(success);
            // the repo is not working at all, therefore one exception will be logged and success is false
            Assert.Equal(1, exceptionCounter.CurrentCount);

            repoMock.Verify(
                r => r.SaveProjectionStateAsync(
                    ItShould.BeEquivalentTo(state),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Try_to_save_projection_state_with_null_as_state_should_work()
        {
            // arrange
            var repoMock = new Mock<IProjectionStateRepository>();
            IProjectionStateRepository sut = repoMock.Object;
            ILogger logger = GetLogger();

            // act
            bool success = await sut.TrySaveProjectionStateAsync(null, null, logger);

            // assert
            Assert.False(success);
        }

        [Fact]
        public async Task Try_to_save_projection_state_with_null_as_repo_should_fail()
        {
            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => (null as IProjectionStateRepository).TrySaveProjectionStateAsync(new ProjectionState()));
        }

        private class CountingHelper
        {
            private int _CurrentCount;

            public int CurrentCount => _CurrentCount;

            public void IncrementCounter(
                LogLevel logLevel)
            {
                if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                {
                    Interlocked.Increment(ref _CurrentCount);
                }
            }
        }
    }
}
