using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Sync.Abstraction.Stores;
using UserProfileService.Sync.Messages.Responses;
using UserProfileService.Sync.Services;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Services
{
    public class ScheduleServiceTests
    {
        [Fact]
        public async Task GetScheduleAsyncSuccess()
        {
            // Arrange
            var expectedSchedule = new SyncSchedule
                                   {
                                       IsActive = true,
                                       ModifiedAt = DateTime.UtcNow,
                                       ModifiedBy = "test"
                                   };

            var store = new Mock<IScheduleStore>();
            var messageBus = new Mock<IBus>();
            ILogger<ScheduleService> logger = new LoggerFactory().CreateLogger<ScheduleService>();

            store.Setup(s => s.GetScheduleAsync(default)).ReturnsAsync(expectedSchedule);

            var service = new ScheduleService(store.Object, messageBus.Object, logger);

            // Act
            SyncSchedule result = await service.GetScheduleAsync();

            // Arrange
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(expectedSchedule);
        }

        [Fact]
        public async Task GetScheduleAsyncSuccess_IfRepoScheduleIsNull()
        {
            // Arrange
            DateTime start = DateTime.UtcNow;

            var store = new Mock<IScheduleStore>();
            var messageBus = new Mock<IBus>();
            ILogger<ScheduleService> logger = new LoggerFactory().CreateLogger<ScheduleService>();

            var service = new ScheduleService(store.Object, messageBus.Object, logger);

            // Act
            SyncSchedule result = await service.GetScheduleAsync();

            // Arrange
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.True(start < result.ModifiedAt);
            Assert.Null(result.ModifiedBy);
        }

        [Fact]
        public async Task ChangeScheduleAsync_Success()
        {
            // Arrange
            DateTime start = DateTime.UtcNow;
            var userId = Guid.NewGuid().ToString();

            var request = new ScheduleRequest(true);

            var store = new Mock<IScheduleStore>();
            var messageBus = new Mock<IBus>();
            ILogger<ScheduleService> logger = new LoggerFactory().CreateLogger<ScheduleService>();

            store.Setup(r => r.SaveScheduleAsync(It.IsAny<SyncSchedule>(), default))
                 .ReturnsAsync((SyncSchedule schedule, CancellationToken ctx) => schedule);

            var service = new ScheduleService(store.Object, messageBus.Object, logger);

            // Act
            SyncSchedule result = await service.ChangeScheduleAsync(request, userId);

            // Arrange
            Assert.Equal(request.IsActive, result.IsActive);
            Assert.True(start < result.ModifiedAt);
            Assert.Equal(userId, result.ModifiedBy);

            messageBus.Verify(
                t => t.Publish(
                    It.Is<SyncScheduleStatus>(t => t.IsActive == request.IsActive),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangeScheduleAsync_Success_IfMessageBroker_Throw_Exception()
        {
            // Arrange
            DateTime start = DateTime.UtcNow;
            var userId = Guid.NewGuid().ToString();

            var request = new ScheduleRequest(true);

            var store = new Mock<IScheduleStore>();
            var messageBus = new Mock<IBus>();
            ILogger<ScheduleService> logger = new LoggerFactory().CreateLogger<ScheduleService>();

            store.Setup(r => r.SaveScheduleAsync(It.IsAny<SyncSchedule>(), default))
                 .ReturnsAsync((SyncSchedule schedule, CancellationToken ctx) => schedule);

            messageBus.Setup(
                          t => t.Publish(
                              It.Is<SyncScheduleStatus>(
                                  status => status.IsActive == request.IsActive && status.CorrelationId != null),
                              It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("test"));

            var service = new ScheduleService(store.Object, messageBus.Object, logger);

            // Act
            SyncSchedule result = await service.ChangeScheduleAsync(request, userId);

            // Arrange
            Assert.Equal(request.IsActive, result.IsActive);
            Assert.True(start < result.ModifiedAt);
            Assert.Equal(userId, result.ModifiedBy);

            messageBus.Verify(
                t => t.Publish(
                    It.Is<SyncScheduleStatus>(
                        status => status.IsActive == request.IsActive && status.CorrelationId != null),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangeScheduleAsync_Should_Throw_ArgumentNullException_IfScheduleIsNull()
        {
            // Arrange
            var store = new Mock<IScheduleStore>();
            var messageBus = new Mock<IBus>();
            ILogger<ScheduleService> logger = new LoggerFactory().CreateLogger<ScheduleService>();
            var service = new ScheduleService(store.Object, messageBus.Object, logger);

            // Act & Arrange
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.ChangeScheduleAsync(null));
        }
    }
}
