using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.Commands;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.EventCollector.Abstractions.Messages.Responses;
using UserProfileService.EventCollector.Configuration;
using Xunit;

namespace UserProfileService.EventCollector.UnitTests
{
    public class EventCollectorAgentTests
    {
        private readonly ILogger<EventCollectorAgent> _Logger;
        private readonly Mock<IEventCollectorStore> _MockEventCollectorStore;
        private readonly IServiceProvider _ServiceProvider;

        public EventCollectorAgentTests()
        {
            _Logger = new NullLogger<EventCollectorAgent>();
            _MockEventCollectorStore = new Mock<IEventCollectorStore>();

            _ServiceProvider = new ServiceCollection()
                               .AddSingleton(_MockEventCollectorStore.Object)
                               .BuildServiceProvider();
        }

        private static IList<EventData> GenerateEventData(
            Guid collectingId,
            IList<SubmitCommandSuccess> commandSuccesses,
            IList<SubmitCommandFailure> commandFailures
        )
        {
            IList<EventData> eventDataCollection = new List<EventData>();

            foreach (SubmitCommandSuccess submitCommandSuccess in commandSuccesses)
            {
                eventDataCollection.Add(
                    new EventData
                    {
                        CollectingId = collectingId,
                        Data = JsonSerializer.Serialize(submitCommandSuccess),
                        ErrorOccurred = false,
                        Host = Guid.NewGuid().ToString(),
                        RequestId = Guid.NewGuid().ToString()
                    });
            }

            foreach (SubmitCommandFailure submitCommandFailure in commandFailures)
            {
                eventDataCollection.Add(
                    new EventData
                    {
                        CollectingId = collectingId,
                        Data = JsonSerializer.Serialize(submitCommandFailure),
                        ErrorOccurred = true,
                        Host = Guid.NewGuid().ToString(),
                        RequestId = Guid.NewGuid().ToString()
                    });
            }

            return eventDataCollection;
        }

        [Fact]
        public async Task ConsumeStartCollecting_Should_Throw_ArgumentNullException_IfContextIsNull()
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);
            ConsumeContext<StartCollectingMessage> context = null;

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => agent.Consume(context));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeStartCollecting_Should_work(StartCollectingMessage contextMessage)
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<StartCollectingMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);
            contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act
            await agent.Consume(contextMock.Object);

            //Assert
            _MockEventCollectorStore.Verify(
                m => m.SaveEntityAsync(
                    It.Is<StartCollectingEventData>(
                        e => e.CollectingId == contextMessage.CollectingId
                            && e.ExternalProcessId == contextMessage.ExternalProcessId
                            && e.CollectItemsAccount == contextMessage.CollectItemsAccount),
                    It.Is<string>(cId => cId == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            contextMock.Verify(
                c => c.Publish(
                    It.Is<StartCollectingEventSuccess>(
                        m => m.CollectingId.Value == contextMessage.CollectingId
                            && m.ExternalProcessId == contextMessage.ExternalProcessId),
                    It.IsAny<CancellationToken>()));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeStartCollecting_Should_Throw_ArgumentException_IfCountLessThanZero(
            StartCollectingMessage contextMessage)
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<StartCollectingMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            contextMessage.CollectItemsAccount = -1;

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Fact]
        public async Task ConsumeStartCollecting_Should_Throw_ArgumentException_When_Message_null(
        )
        {
            // Arrange
            StartCollectingMessage contextMessage = null;

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<StartCollectingMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeStartCollecting_Should_Throw_ArgumentException_IfCollectingIdEmpty(
            StartCollectingMessage contextMessage)
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<StartCollectingMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            contextMessage.CollectingId = Guid.Empty;

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Fact]
        public async Task ConsumeSetCollecting_Should_Throw_ArgumentNullException_IfContextIsNull()
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);
            ConsumeContext<SetCollectItemsAccountMessage> context = null;

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => agent.Consume(context));
        }

        [Fact]
        public async Task ConsumeSetCollecting_Should_Throw_ArgumentException_When_Message_null(
        )
        {
            // Arrange
            SetCollectItemsAccountMessage contextMessage = null;

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<SetCollectItemsAccountMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeSetCollecting_Should_Throw_ArgumentException_IfCountLessThanZero(
            SetCollectItemsAccountMessage contextMessage)
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<SetCollectItemsAccountMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            contextMessage.CollectItemsAccount = -1;

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeSetCollecting_Should_Throw_ArgumentException_IfCollectingIdEmpty(
            SetCollectItemsAccountMessage contextMessage)
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<SetCollectItemsAccountMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            contextMessage.CollectingId = Guid.Empty;

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [AutoDataWithoutExceptionInformation]
        public async Task ConsumeSetCollectingAccount_Should_call_repo_with_right_param(
            SetCollectItemsAccountMessage contextMessage,
            IList<SubmitCommandSuccess> commandSuccesses,
            IList<SubmitCommandFailure> commandFailures)
        {
            // Arrange
            IList<EventData> eventDataCollection =
                GenerateEventData(contextMessage.CollectingId, commandSuccesses, commandFailures);

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var externalProcessId = Guid.NewGuid();

            contextMessage.CollectItemsAccount =
                contextMessage.CollectItemsAccount <= 0 ? 5 : contextMessage.CollectItemsAccount;

            var contextMock = new Mock<ConsumeContext<SetCollectItemsAccountMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);
            contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            _MockEventCollectorStore.Setup(
                                        m => m.TrySetCollectingItemsAmountAsync(
                                            It.IsAny<Guid>(),
                                            It.IsAny<int>(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(true);

            _MockEventCollectorStore
                .Setup(m => m.GetCountOfEventDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMessage.CollectItemsAccount);

            _MockEventCollectorStore.Setup(m => m.GetEventData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(eventDataCollection);

            _MockEventCollectorStore.Setup(
                                        m => m.GetExternalProcessIdAsync(
                                            It.IsAny<string>(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(externalProcessId.ToString);

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act
            await agent.Consume(contextMock.Object);

            //Assert
            _MockEventCollectorStore.Verify(
                m => m.TrySetCollectingItemsAmountAsync(
                    It.Is<Guid>(e => e == contextMessage.CollectingId),
                    It.Is<int>(e => e == contextMessage.CollectItemsAccount),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _MockEventCollectorStore.Verify(
                m => m.GetCountOfEventDataAsync(
                    It.Is<string>(e => e == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            _MockEventCollectorStore.Verify(
                m => m.GetExternalProcessIdAsync(
                    It.Is<string>(e => e == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            contextMock.Verify(
                c => c.Publish(
                    It.Is<CollectingItemsStatus>(
                        m => m.CollectingId == contextMessage.CollectingId
                            && m.ExternalProcessId == externalProcessId.ToString()),
                    It.IsAny<CancellationToken>()));

            _MockEventCollectorStore.Verify(
                m => m.GetEventData(
                    It.Is<string>(e => e == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            contextMock.Verify(
                c => c.Publish(
                    It.Is<CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>>(
                        m => m.CollectingId == contextMessage.CollectingId
                            && m.ExternalProcessId == externalProcessId.ToString()
                            && m.Failures.Count + m.Successes.Count == eventDataCollection.Count),
                    It.IsAny<CancellationToken>()));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeGetItemsStatus_Should_work(GetCollectingItemsStatusMessage contextMessage)
        {
            //Arrange

            var externalProcessId = Guid.NewGuid();
            var collectedItemsAccount = 10;

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            _MockEventCollectorStore
                .Setup(m => m.GetCountOfEventDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(collectedItemsAccount);

            _MockEventCollectorStore.Setup(
                                        m => m.GetExternalProcessIdAsync(
                                            It.IsAny<string>(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(externalProcessId.ToString);

            var contextMock = new Mock<ConsumeContext<GetCollectingItemsStatusMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);
            contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            //Act
            await agent.Consume(contextMock.Object);

            // Assert
            _MockEventCollectorStore.Verify(
                m => m.GetCountOfEventDataAsync(
                    It.Is<string>(e => e == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _MockEventCollectorStore.Verify(
                m => m.GetExternalProcessIdAsync(
                    It.Is<string>(e => e == contextMessage.CollectingId.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            contextMock.Verify(
                c => c.Publish(
                    It.Is<CollectingItemsStatus>(
                        m => m.CollectingId == contextMessage.CollectingId
                            && m.ExternalProcessId == externalProcessId.ToString()),
                    It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task ConsumeGetCollecting_Should_Throw_ArgumentNullException_IfContextIsNull()
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);
            ConsumeContext<GetCollectingItemsStatusMessage> context = null;

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => agent.Consume(context));
        }

        [Fact]
        public async Task ConsumeGetCollecting_Should_Throw_ArgumentException_When_Message_null(
        )
        {
            // Arrange
            GetCollectingItemsStatusMessage contextMessage = null;

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var contextMock = new Mock<ConsumeContext<GetCollectingItemsStatusMessage>>();
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [AutoData]
        public async Task ConsumeGetCollecting_Should_Throw_ArgumentNullException_IfCollectingIdIsEmpty(
            GetCollectingItemsStatusMessage contextMessage)
        {
            // Arrange
            var contextMock = new Mock<ConsumeContext<GetCollectingItemsStatusMessage>>();
            contextMessage.CollectingId = Guid.Empty;
            contextMock.Setup(c => c.Message).Returns(contextMessage);

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(contextMock.Object));
        }

        [Theory]
        [InlineData(500, 100, 100, 1)]
        [InlineData(500, 100, 99, 0)]
        [InlineData(500, 100, 75, 0)]
        [InlineData(500, 100, 50, 1)]
        [InlineData(500, 100, 1, 1)]
        [InlineData(500, 100, 120, 0)]
        public async Task ConsumeMessage_Should_PublishStatus_IfModuloIsZero_ElseNoPublish(
            int itemsAccount,
            int eventDataCount,
            int modulo,
            int publishTimes)
        {
            // Arrange
            var message = new SubmitCommandSuccess("test-command", Guid.NewGuid().ToString(), Guid.NewGuid());

            var contextMock = new Mock<ConsumeContext<SubmitCommandSuccess>>();
            contextMock.SetupGet(x => x.Message).Returns(message);
            contextMock.SetupGet(x => x.Host.MachineName).Returns("machine-1");

            _MockEventCollectorStore.Setup(
                                        e => e.GetCountOfEventDataAsync(
                                            It.Is<string>(t => t == message.CollectingId.ToString()),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(eventDataCount);

            var startCollectingEventData = new StartCollectingEventData
                                           {
                                               CollectingId = message.CollectingId,
                                               StartedAt = DateTime.UtcNow.AddMinutes(-5),
                                               StatusDispatch = new StatusDispatch(modulo),
                                               CollectItemsAccount = itemsAccount,
                                               ExternalProcessId = Guid.NewGuid().ToString()
                                           };

            _MockEventCollectorStore.Setup(
                                        e => e.GetEntityAsync<StartCollectingEventData>(
                                            message.CollectingId.ToString(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(startCollectingEventData);

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act
            await agent.Consume(contextMock.Object);

            // Assert
            contextMock.Verify(
                cm => cm.Publish(
                    It.Is<CollectingItemsStatus>(
                        c => c.CollectedItemsAccount == eventDataCount
                            && c.ExternalProcessId == startCollectingEventData.ExternalProcessId
                            && c.CollectingId == startCollectingEventData.CollectingId),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(publishTimes));

            _MockEventCollectorStore.Verify(
                e => e.GetEventData(startCollectingEventData.CollectingId.ToString(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(500, 100, 600, 0)]
        [InlineData(500, 500, 1, 1)]
        [InlineData(500, 500, 120, 1)]
        public async Task ConsumeMessage_Should_PublishResponse_IfAccountIsEqualToDataCount_ElseNoPublish(
            int itemsAccount,
            int eventDataCount,
            int modulo,
            int publishTimes)
        {
            // Arrange
            var message = new SubmitCommandSuccess("test-command", Guid.NewGuid().ToString(), Guid.NewGuid());

            var contextMock = new Mock<ConsumeContext<SubmitCommandSuccess>>();
            contextMock.SetupGet(x => x.Message).Returns(message);
            contextMock.SetupGet(x => x.Host.MachineName).Returns("machine-1");

            _MockEventCollectorStore.Setup(
                                        e => e.GetCountOfEventDataAsync(
                                            It.Is<string>(t => t == message.CollectingId.ToString()),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(eventDataCount);

            var startCollectingEventData = new StartCollectingEventData
                                           {
                                               CollectingId = message.CollectingId,
                                               StartedAt = DateTime.UtcNow.AddMinutes(-5),
                                               StatusDispatch = new StatusDispatch(modulo),
                                               CollectItemsAccount = itemsAccount,
                                               ExternalProcessId = Guid.NewGuid().ToString()
                                           };

            _MockEventCollectorStore.Setup(
                                        e => e.GetEntityAsync<StartCollectingEventData>(
                                            message.CollectingId.ToString(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(startCollectingEventData);

            _MockEventCollectorStore.Setup(
                                        e => e.GetEventData(
                                            startCollectingEventData.CollectingId.ToString(),
                                            It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(new List<EventData>());

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new EventCollectorAgent(_ServiceProvider, _Logger, options.Object);

            // Act
            await agent.Consume(contextMock.Object);

            // Assert
            contextMock.Verify(
                cm => cm.Publish(
                    It.Is<CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>>(
                        c => c.ExternalProcessId == startCollectingEventData.ExternalProcessId
                            && c.CollectingId == startCollectingEventData.CollectingId),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            contextMock.Verify(
                cm => cm.Publish(
                    It.Is<CollectingItemsStatus>(
                        c => c.CollectedItemsAccount == eventDataCount
                            && c.ExternalProcessId == startCollectingEventData.ExternalProcessId
                            && c.CollectingId == startCollectingEventData.CollectingId),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _MockEventCollectorStore.Verify(
                e => e.GetEventData(startCollectingEventData.CollectingId.ToString(), It.IsAny<CancellationToken>()),
                Times.Exactly(publishTimes));
        }
    }
}
