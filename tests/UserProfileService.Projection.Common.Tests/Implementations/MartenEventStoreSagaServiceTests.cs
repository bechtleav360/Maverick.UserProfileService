using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Implementations;
using Xunit;

namespace UserProfileService.Projection.Common.Tests.Implementations
{
    public class MartenEventStoreSagaServiceTests
    {
        private Mock<IFirstProjectionEventLogWriter> SetupIFirstProjectionEventLogWriter(EventBatch batch)
        {
            var firstProjectionEventLogWriterMock = new Mock<IFirstProjectionEventLogWriter>();

            firstProjectionEventLogWriterMock.Setup(s => s.GetBatchAsync(batch.Id, default))
                .ReturnsAsync(batch);

            return firstProjectionEventLogWriterMock;
        }

        private (EventBatch batch, Guid id) CreateBatch(EventStatus status = EventStatus.Initialized)
        {
            var id = Guid.NewGuid();

            var batch = new EventBatch
            {
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Id = id.ToString()
            };

            return (batch, id);
        }

        #region CreateBatchAsync

        [Fact]
        public async Task CreateBatchAsync_Success()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            var eventStoreClientMock = new Mock<IEventStorageClient>();

            Mock<IFirstProjectionEventLogWriter> firstProjectionEventLogWriterMock =
                SetupIFirstProjectionEventLogWriter(batch);

            firstProjectionEventLogWriterMock.Setup(t => t.CreateBatchAsync(It.IsAny<string>(), default))
                .ReturnsAsync(batch);

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                eventStoreClientMock.Object,
                logger);

            // Act
            Guid result = await eventStore.CreateBatchAsync();

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            firstProjectionEventLogWriterMock.Verify(
                t => t.CreateBatchAsync(result.ToString(), default),
                Times.Once);
        }

        [Fact]
        public async Task CreateBatchAsync_WithEvents_Success()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            var events = new List<EventTuple>
            {
                new EventTuple(
                    "TestStream",
                    new GroupCreatedEvent
                    {
                        EventId = Guid.NewGuid().ToString()
                    })
            };

            var eventStorageClient = new Mock<IEventStorageClient>();
            eventStorageClient.Setup(e => e.ValidateEventsAsync(events, default)).ReturnsAsync(true);

            IList<EventLogTuple> eventLogTuples = null;

            Mock<IFirstProjectionEventLogWriter> firstProjectionEventLogWriterMock =
                SetupIFirstProjectionEventLogWriter(batch);

            firstProjectionEventLogWriterMock.Setup(
                    s => s.CreateBatchAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<EventLogTuple>>(),
                        default))
                .ReturnsAsync(batch)
                .Callback<string, IList<EventLogTuple>, CancellationToken>(
                    (id, eventsToBeAdded, _) =>
                    {
                        eventLogTuples = eventsToBeAdded;
                        batch.Id = id;
                    });

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                eventStorageClient.Object,
                logger);

            // Act
            Guid result = await eventStore.CreateBatchAsync(default, events.ToArray());

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            eventStorageClient.Verify(t => t.ValidateEventsAsync(events, default), Times.Once);

            Assert.NotNull(eventLogTuples);
            Assert.All(events, e => Assert.Contains(eventLogTuples, elt => elt.Id == e.Event.EventId));

            firstProjectionEventLogWriterMock.Verify(
                t => t.CreateBatchAsync(result.ToString(), eventLogTuples, default),
                Times.Once);
        }

        [Fact]
        public async Task CreateBatchAsync_WithEvents_ThrowException_IfEventsAreNull()
        {
            // Arrange
            var eventStoreMock = new Mock<IEventStorageClient>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                eventStoreMock.Object,
                logger);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => eventStore.CreateBatchAsync(default, null));
        }

        [Fact]
        public async Task CreateBatchAsync_WithEvents_ThrowException_IfEventsAreEmpty()
        {
            // Arrange
            var eventStoreMock = new Mock<IEventStorageClient>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                eventStoreMock.Object,
                logger);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => eventStore.CreateBatchAsync(default, Array.Empty<EventTuple>()));
        }

        [Fact]
        public async Task CreateBatchAsync_WithEvents_ThrowException_IfEventsInvalid()
        {
            // Arrange
            var events = new List<EventTuple>
            {
                new EventTuple(
                    "TestStream",
                    new GroupCreatedEvent
                    {
                        EventId = Guid.NewGuid().ToString()
                    })
            };

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            firstProjectionEventStoreClientMock.Setup(s => s.ValidateEventsAsync(events, default))
                .ReturnsAsync(false);

            var firstProjectionEventLogWriterMock = new Mock<IFirstProjectionEventLogWriter>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => eventStore.CreateBatchAsync(default, events.ToArray()));

            // Assert

            firstProjectionEventStoreClientMock.Verify(t => t.ValidateEventsAsync(events, default), Times.Once);

            firstProjectionEventLogWriterMock.Verify(
                t => t.AddEventsAsync(It.IsAny<string>(), It.IsAny<IList<EventLogTuple>>(), default),
                Times.Never);
        }

        #endregion

        #region ExecuteBatchAsync

        [Fact]
        public async Task ExecuteBatchAsync_Success()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            var events = new List<EventLogTuple>
            {
                new EventLogTuple(
                    new EventTuple(
                        "Test",
                        new GroupCreatedEvent
                        {
                            EventId = Guid.NewGuid().ToString()
                        }),
                    batch.Id),
                new EventLogTuple(
                    new EventTuple(
                        "Test",
                        new GroupCreatedEvent
                        {
                            EventId = Guid.NewGuid().ToString()
                        }),
                    batchId.ToString())
            };

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

           
            Mock<IFirstProjectionEventLogWriter> firstProjectionEventLogWriterMock =
                SetupIFirstProjectionEventLogWriter(batch);

            firstProjectionEventLogWriterMock.Setup(t => t.GetEventsAsync(batchId.ToString(), default))
                .ReturnsAsync(events);

            var outboxProcessorMock = new Mock<IOutboxProcessorService>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await eventStore.ExecuteBatchAsync(batchId);

            // Arrange
            firstProjectionEventLogWriterMock.Verify(s => s.GetBatchAsync(batchId.ToString(), default), Times.Once);

            outboxProcessorMock.Verify(s => s.CheckAndProcessEvents(default), Times.Never);
        }

        [Fact]
        public async Task ExecuteBatchAsync_Success_IfEmptyEvents()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            IEnumerable<EventLogTuple> events = new List<EventLogTuple>();

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            Mock<IFirstProjectionEventLogWriter> firstProjectionEventLogWriterMock =
                SetupIFirstProjectionEventLogWriter(batch);

            var outboxProcessorMock = new Mock<IOutboxProcessorService>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await eventStore.ExecuteBatchAsync(batchId);

            // Arrange
            outboxProcessorMock.Verify(s => s.CheckAndProcessEvents(default), Times.Never);
        }

        [Fact]
        public async Task ExecuteBatchAsync_ThrowException_IfBatchIdIsEmpty()
        {
            // Arrange
            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();
            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(() => eventStore.ExecuteBatchAsync(Guid.Empty));
        }

        #endregion

        #region AddEventsAsync

        [Fact]
        public async Task AddEventsAsync_Success()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            var events = new List<EventTuple>
            {
                new EventTuple(
                    "TestStream",
                    new GroupCreatedEvent
                    {
                        EventId = Guid.NewGuid().ToString()
                    })
            };

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            firstProjectionEventStoreClientMock.Setup(s => s.ValidateEventsAsync(events, default))
                .ReturnsAsync(true);


            IList<EventLogTuple> eventLogTuples = null;
            var firstProjectionEventLogWriterMock = new Mock<IFirstProjectionEventLogWriter>();

            firstProjectionEventLogWriterMock.Setup(
                    t => t.AddEventsAsync(
                        batchId.ToString(),
                        It.IsAny<IList<EventLogTuple>>(),
                        default))
                .Callback<string, IList<EventLogTuple>, CancellationToken>(
                    (id, eventsToBeAdded, _) => eventLogTuples = eventsToBeAdded);

            firstProjectionEventLogWriterMock.Setup(s => s.GetBatchAsync(batchId.ToString(), default))
                .ReturnsAsync(batch);


            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await eventStore.AddEventsAsync(batchId, events);

            // Assert

            firstProjectionEventStoreClientMock.Verify(t => t.ValidateEventsAsync(events, default), Times.Once);

            Assert.NotNull(eventLogTuples);
            Assert.All(events, e => Assert.Contains(eventLogTuples, elt => elt.Id == e.Event.EventId));

            firstProjectionEventLogWriterMock.Verify(
                t => t.AddEventsAsync(batchId.ToString(), eventLogTuples, default),
                Times.Once);
        }

        [Fact]
        public async Task AddEventsAsync_ThrowException_IfEventsAreNull()
        {
            // Arrange
            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                firstProjectionEventStoreClientMock.Object,
            logger);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => eventStore.AddEventsAsync(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task AddEventsAsync_ThrowException_IfEventsAreEmpty()
        {
            // Arrange
            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => eventStore.AddEventsAsync(Guid.NewGuid(), Array.Empty<EventTuple>()));
        }

        [Fact]
        public async Task AddEventsAsync_ThrowException_IfEventsInvalid()
        {
            // Arrange
            (EventBatch batch, Guid batchId) = CreateBatch();

            var events = new List<EventTuple>
            {
                new EventTuple(
                    "TestStream",
                    new GroupCreatedEvent
                    {
                        EventId = Guid.NewGuid().ToString()
                    })
            };

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();

            firstProjectionEventStoreClientMock.Setup(s => s.ValidateEventsAsync(events, default))
                .ReturnsAsync(false);


            var firstProjectionEventLogWriterMock = new Mock<IFirstProjectionEventLogWriter>();

            firstProjectionEventLogWriterMock.Setup(s => s.GetBatchAsync(batchId.ToString(), default))
                .ReturnsAsync(batch);


            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(() => eventStore.AddEventsAsync(batchId, events));

            // Assert
            firstProjectionEventStoreClientMock.Verify(t => t.ValidateEventsAsync(events, default), Times.Once);

            firstProjectionEventLogWriterMock.Verify(
                t => t.AddEventsAsync(batchId.ToString(), It.IsAny<IList<EventLogTuple>>(), default),
                Times.Never);
        }

        #endregion

        #region AbortBatchAsync

        [Fact]
        public async Task AbortBatchAsync_Success()
        {
            // Arrange

            (EventBatch batch, Guid batchId) = CreateBatch();

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();
            
            Mock<IFirstProjectionEventLogWriter> firstProjectionEventLogWriterMock =
                SetupIFirstProjectionEventLogWriter(batch);

            var outboxProcessorMock = new Mock<IOutboxProcessorService>();

            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                firstProjectionEventLogWriterMock.Object,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act
            await eventStore.AbortBatchAsync(batchId);

            // Assert
            firstProjectionEventLogWriterMock.Verify(t => t.AbortBatchAsync(batchId.ToString(), default), Times.Once);
        }

        [Fact]
        public async Task AbortBatchAsync_ThrowException_ifBatchIdIsEmpty()
        {
            // Arrange

            var firstProjectionEventStoreClientMock = new Mock<IEventStorageClient>();
            ILogger<MartenEventStoreSagaService> logger =
                new LoggerFactory().CreateLogger<MartenEventStoreSagaService>();

            var eventStore = new MartenEventStoreSagaService(
                null,
                firstProjectionEventStoreClientMock.Object,
                logger);

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => eventStore.AbortBatchAsync(Guid.Empty));
        }

        #endregion
    }
}
