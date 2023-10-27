using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Configuration;
using UserProfileService.Validation.Abstractions;
using UserProfileService.Validation.Abstractions.Message;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests
{
    public class ValidationEventCollectorAgentTests
    {
        private readonly ILogger<ValidationEventCollectorAgent> _logger;
        private readonly Mock<IEventCollectorStore> _mockEventCollectorStore;
        private readonly IServiceProvider _serviceProvider;

        public ValidationEventCollectorAgentTests()
        {
            _mockEventCollectorStore = new Mock<IEventCollectorStore>();

            _serviceProvider = new ServiceCollection()
                .AddSingleton(_mockEventCollectorStore.Object)
                .BuildServiceProvider();

            _logger =
                new LoggerFactory().CreateLogger<ValidationEventCollectorAgent>();
        }

        [Fact]
        public async Task Consume_Should_Throw_ArgumentNullException_IfContextIsNull()
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new ValidationEventCollectorAgent(_serviceProvider, _logger, options.Object);

            // Act && Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            await Assert.ThrowsAsync<ArgumentNullException>(() => agent.Consume(null));
        }

        [Fact]
        public async Task Consume_Should_Throw_ArgumentException_IfMessageIsNull()
        {
            // Arrange
            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            var agent = new ValidationEventCollectorAgent(_serviceProvider, _logger, options.Object);

            var consumeContext = new Mock<ConsumeContext<ValidationResponse>>();
            consumeContext.SetupGet(t => t.Message).Returns((ValidationResponse)null);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => agent.Consume(consumeContext.Object));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        public async Task Consume_Success(int expectedMessages, int publishTimes)
        {
            // Arrange
            var expectedIsValid = false;
            var requestId = Guid.NewGuid();
            var expectedValidationAttribute = new ValidationAttribute("member", "message");

            var message = new ValidationResponse(false)
                          {
                              Errors = new List<ValidationAttribute>
                                       {
                                           expectedValidationAttribute
                                       },
                              CollectingId = requestId
                          };

            ICollection<EventData> eventDataList = new List<EventData>
            {
                new EventData
                {
                    Data = JsonSerializer.Serialize(message),
                    CollectingId = requestId
                }
            };

            var consumeContext = new Mock<ConsumeContext<ValidationResponse>>();
            consumeContext.SetupGet(s => s.RequestId).Returns(requestId);
            consumeContext.SetupGet(s => s.Message).Returns(message);
            consumeContext.SetupGet(s => s.Host).Returns(new BusHostInfo());

            var configuration = new EventCollectorConfiguration
            {
                ExpectedResponses = expectedMessages
            };

            var options =
                new Mock<IOptionsMonitor<EventCollectorConfiguration>>();

            options.Setup(o => o.CurrentValue).Returns(configuration);

            _mockEventCollectorStore.Setup(
                    s => s.SaveEventDataAsync(
                        It.Is<EventData>(e => e.CollectingId == requestId),
                        default))
                .Callback<EventData, CancellationToken>(
                    (e, _) => eventDataList = new List<EventData>
                    {
                        e
                    })
                .ReturnsAsync(1);

            ValidationCompositeResponse currentPublishedMessage = null;

            consumeContext.Setup(
                    c => c.Publish(
                        It.Is<ValidationCompositeResponse>(v => v.IsValid == expectedIsValid),
                        default))
                .Callback<ValidationCompositeResponse, CancellationToken>((r, _) => currentPublishedMessage = r);

            _mockEventCollectorStore.Setup(s => s.GetEventData(requestId.ToString("D"), default))
                .ReturnsAsync(eventDataList);

            _mockEventCollectorStore.Setup(
                    s
                        => s.GetCountOfEventDataAsync(
                            requestId.ToString("D"),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventDataList.Count);

            var agent = new ValidationEventCollectorAgent(_serviceProvider, _logger, options.Object);

            // Act
            await agent.Consume(consumeContext.Object);

            // Assert
            consumeContext.Verify(
                c => c.Publish(
                    It.Is<ValidationCompositeResponse>(v => v.IsValid == expectedIsValid),
                    default),
                Times.Exactly(publishTimes));

            if (publishTimes == 1)
            {
                EventData eventData = Assert.Single(eventDataList);
                Assert.Equal(requestId, eventData.CollectingId);

                Assert.NotNull(currentPublishedMessage);
                Assert.Equal(expectedIsValid, currentPublishedMessage.IsValid);

                ValidationAttribute validationAttribute = Assert.Single(currentPublishedMessage.Errors);
                expectedValidationAttribute.Should().BeEquivalentTo(validationAttribute);
            }
        }
    }
}
