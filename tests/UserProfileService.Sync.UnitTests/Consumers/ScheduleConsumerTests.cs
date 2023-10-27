using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Consumers;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Messages.Responses;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Consumers
{
    public class ScheduleConsumerTests
    {
        [Fact]
        public async Task Consume_SetSyncScheduleCommand_Success()
        {
            // Arrange
            var scheduleService = new Mock<IScheduleService>();

            await using ServiceProvider provider = new ServiceCollection()
                                                   .AddSingleton(scheduleService.Object)
                                                   .AddLogging()
                                                   .AddMassTransitTestHarness(
                                                       cfg => { cfg.AddConsumer<ScheduleConsumer>(); })
                                                   .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var message = new SetSyncScheduleCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "test",
                              IsActive = true
                          };

            // Act
            await harness.Bus.Publish(message);

            // Assert
            Assert.True(await harness.Consumed.Any<SetSyncScheduleCommand>());

            IConsumerTestHarness<ScheduleConsumer> consumerHarness = harness.GetConsumerHarness<ScheduleConsumer>();

            Assert.True(await consumerHarness.Consumed.Any<SetSyncScheduleCommand>());
            Assert.True(await harness.Published.Any<SetSyncScheduleSuccess>());

            scheduleService
                .Verify(
                    s => s.ChangeScheduleAsync(
                        It.Is<ScheduleRequest>(t => t.IsActive == message.IsActive),
                        message.InitiatorId,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Consume_SetSyncScheduleCommand_Should_SendFailureMessage_IfErrorOccurred()
        {
            // Arrange
            var scheduleService = new Mock<IScheduleService>();

            await using ServiceProvider provider = new ServiceCollection()
                                                   .AddSingleton(scheduleService.Object)
                                                   .AddLogging()
                                                   .AddMassTransitTestHarness(
                                                       cfg => { cfg.AddConsumer<ScheduleConsumer>(); })
                                                   .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var message = new SetSyncScheduleCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "test",
                              IsActive = true
                          };

            scheduleService
                .Setup(
                    s => s.ChangeScheduleAsync(
                        It.Is<ScheduleRequest>(t => t.IsActive == message.IsActive),
                        message.InitiatorId,
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("test"));

            // Act
            await harness.Bus.Publish(message);

            // Assert
            Assert.True(await harness.Consumed.Any<SetSyncScheduleCommand>());

            IConsumerTestHarness<ScheduleConsumer> consumerHarness = harness.GetConsumerHarness<ScheduleConsumer>();

            Assert.True(await consumerHarness.Consumed.Any<SetSyncScheduleCommand>());
            Assert.True(await harness.Published.Any<SetSyncScheduleFailure>());

            scheduleService
                .Verify(
                    s => s.ChangeScheduleAsync(
                        It.Is<ScheduleRequest>(t => t.IsActive == message.IsActive),
                        message.InitiatorId,
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Consume_SetSyncScheduleCommand_Should_ThrowError_IfIsActiveIsNull()
        {
            // Arrange
            var scheduleService = new Mock<IScheduleService>();

            await using ServiceProvider provider = new ServiceCollection()
                                                   .AddSingleton(scheduleService.Object)
                                                   .AddLogging()
                                                   .AddMassTransitTestHarness(
                                                       cfg => { cfg.AddConsumer<ScheduleConsumer>(); })
                                                   .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var message = new SetSyncScheduleCommand
                          {
                              CorrelationId = Guid.NewGuid(),
                              InitiatorId = "test",
                              IsActive = null
                          };

            scheduleService
                .Setup(
                    s => s.ChangeScheduleAsync(
                        It.Is<ScheduleRequest>(t => t.IsActive == message.IsActive),
                        message.InitiatorId,
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("test"));

            // Act
            await harness.Bus.Publish(message);

            // Assert
            Assert.True(await harness.Consumed.Any<SetSyncScheduleCommand>());

            IConsumerTestHarness<ScheduleConsumer> consumerHarness = harness.GetConsumerHarness<ScheduleConsumer>();

            Assert.True(await consumerHarness.Consumed.Any<SetSyncScheduleCommand>());
            Assert.True(await harness.Published.Any<SetSyncScheduleFailure>());

            scheduleService
                .Verify(
                    s => s.ChangeScheduleAsync(
                        It.Is<ScheduleRequest>(t => t.IsActive == message.IsActive),
                        message.InitiatorId,
                        It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task Consume_GetSyncScheduleCommand_Success()
        {
            // Arrange
            var syncSchedule = new SyncSchedule
                               {
                                   IsActive = true,
                                   ModifiedAt = DateTime.UtcNow,
                                   ModifiedBy = "test"
                               };

            var scheduleService = new Mock<IScheduleService>();

            scheduleService
                .Setup(s => s.GetScheduleAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(syncSchedule);

            await using ServiceProvider provider = new ServiceCollection()
                                                   .AddSingleton(scheduleService.Object)
                                                   .AddLogging()
                                                   .AddMassTransitTestHarness(
                                                       cfg => { cfg.AddConsumer<ScheduleConsumer>(); })
                                                   .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();

            var message = new GetSyncScheduleCommand();

            // Act
            await harness.Bus.Publish(message);

            // Assert
            Assert.True(await harness.Consumed.Any<GetSyncScheduleCommand>());

            IConsumerTestHarness<ScheduleConsumer> consumerHarness = harness.GetConsumerHarness<ScheduleConsumer>();

            Assert.True(await consumerHarness.Consumed.Any<GetSyncScheduleCommand>());
            Assert.True(await harness.Published.Any<SyncScheduleStatus>());

            scheduleService
                .Verify(
                    s => s.GetScheduleAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
