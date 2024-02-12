using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Projection.Common.Implementations;
using Xunit;

namespace UserProfileService.Projection.Common.UnitTests.Implementations
{
    public class EventStoreOutboxProcessorServiceTests
    {
        [Fact]
        public async Task CheckAndProcessEvents_Success()
        {
            // Arrange
            var databaseClientMock = new Mock<IFirstProjectionEventLogWriter>();
            var eventStoreClientMock = new Mock<IEventStorageClient>();
          
            
            ILogger<MartenEventStoreOutboxProcessorService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreOutboxProcessorService>();

            var firstCtx = new CancellationToken();

            databaseClientMock.SetupSequence(db => db.TryGetNextCommittedBatchAsync(firstCtx))
                              .ReturnsAsync(
                                  (true, new EventBatch
                                  {
                                      Id = Guid.NewGuid().ToString(),
                                      Status = EventStatus.Committed
                                  }))
                              .ReturnsAsync((false, default));

            var processor = new MartenEventStoreOutboxProcessorService(
                databaseClientMock.Object,
                eventStoreClientMock.Object,
                logger);

            // Act
            await processor.CheckAndProcessEvents(firstCtx);

            // Assert
            databaseClientMock.Verify(s => s.TryGetNextCommittedBatchAsync(firstCtx), Times.Exactly(2));
        }
    }
}
