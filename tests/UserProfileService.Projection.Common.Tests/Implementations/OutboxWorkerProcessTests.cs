using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Configurations;
using UserProfileService.Projection.Common.Implementations;
using Xunit;

namespace UserProfileService.Projection.Common.Tests.Implementations
{
    public class OutboxWorkerProcessTests
    {
        [Theory]
        [InlineData(250, 1)]
        [InlineData(750, 2)]
        [InlineData(1250, 3)]
        public async Task ExecuteAsync_Success(int delay, int callCount)
        {
            // Arrange
            var outboxConfiguration = new OutboxConfiguration
            {
                Time = TimeSpan.FromMilliseconds(500)
            };

            ILogger<OutboxWorkerProcess> logger = new LoggerFactory().CreateLogger<OutboxWorkerProcess>();
            var activitySourceWrapperMock = new Mock<IActivitySourceWrapper>();

            var activitySource = new ActivitySource("");
            activitySourceWrapperMock.Setup(s => s.ActivitySource).Returns(activitySource);

            var outboxProcessorMock = new Mock<IOutboxProcessorService>();

            IOptionsMonitor<OutboxConfiguration> config =
                new OutboxConfigurationOptionsMonitor(outboxConfiguration);

            var worker = new OutboxWorkerProcess(
                logger,
                activitySourceWrapperMock.Object,
                outboxProcessorMock.Object,
                config);

            var cts = new CancellationTokenSource();

            // Act
            await worker.StartAsync(cts.Token);

            await Task.Delay(delay);

            await worker.StopAsync(cts.Token);

            // Assert
            activitySourceWrapperMock.Verify(s => s.ActivitySource, Times.Exactly(callCount));

            outboxProcessorMock.Verify(
                s => s.CheckAndProcessEvents(It.IsAny<CancellationToken>()),
                Times.Exactly(callCount));
        }
    }

    internal class OutboxConfigurationOptionsMonitor : IOptionsMonitor<OutboxConfiguration>
    {
        public OutboxConfiguration CurrentValue { get; }

        public OutboxConfigurationOptionsMonitor(OutboxConfiguration currentValue)
        {
            CurrentValue = currentValue;
        }

        public OutboxConfiguration Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<OutboxConfiguration, string> listener)
        {
            listener.Invoke(CurrentValue, "Test");

            return null;
        }
    }
}
