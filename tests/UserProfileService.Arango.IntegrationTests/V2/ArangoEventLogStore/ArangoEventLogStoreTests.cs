using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore
{
    public class ArangoEventLogStoreTests : ArangoFirstLevelRepoTestBase
    {
        protected override string GetArangoId<TType>(string key)
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.NewEventLogStore(FirstLevelProjectionPrefix).ModelsInfo;

            string collection = modelsInfo.GetCollectionName<TType>();

            return GetArangoId(collection, key);
        }

        // ReSharper disable once OptionalParameterHierarchyMismatch
        protected override IArangoDbClient GetArangoClient(string name = ArangoDbClientName)
        {
            return GetServiceProvider()
                .GetRequiredService<IArangoDbClientFactory>()
                .Create(ArangoDbClientEventLogStoreName);
        }

        [Fact]
        public async Task GetNextCommittedBatchAsync_Success()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed),
                new EventBatchTestData("batch-2", EventStatus.Committed)
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            EventBatch nextBatch = await arangoEventLogStore.GetNextCommittedBatchAsync();

            // Assert
            Assert.NotNull(nextBatch);
            Assert.Equal("batch-2", nextBatch.Id);
        }

        [Fact]
        public async Task GetNextCommittedBatchAsync_Success_IfNoBatchFound()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed),
                new EventBatchTestData("batch-2", EventStatus.Aborted)
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<InstanceNotFoundException>(() => arangoEventLogStore.GetNextCommittedBatchAsync());
        }

        [Fact]
        public async Task GetBatchAsync_Success()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed),
                new EventBatchTestData("batch-2", EventStatus.Committed)
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            EventBatch batch = await arangoEventLogStore.GetBatchAsync(testData.First().Id);

            // Assert
            Assert.NotNull(batch);
            Assert.Equal("batch-1", batch.Id);
        }

        [Fact]
        public async Task GetBatchAsync_Should_ThrowException_IfNoBatchFound()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed),
                new EventBatchTestData("batch-2", EventStatus.Aborted)
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => arangoEventLogStore.GetBatchAsync("some-invalid-id"));
        }

        [Fact]
        public async Task GetEventsAsync_Success()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-4"),
                        new EventTestData("test-5"),
                        new EventTestData("test-6")
                    }
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            IEnumerable<EventLogTuple> events = await arangoEventLogStore.GetEventsAsync(testData.First().Id);

            // Assert
            Assert.NotNull(events);
            Assert.Equal(3, events.Count());
        }

        [Fact]
        public async Task GetEventsAsync_Success_IfBatchEmpty()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>()
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            IEnumerable<EventLogTuple> events = await arangoEventLogStore.GetEventsAsync(testData[1].Id);

            // Assert
            Assert.NotNull(events);
            Assert.Empty(events);
        }

        [Fact]
        public async Task GetEventsAsync_Success_IfNoBatches()
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            IEnumerable<EventLogTuple> events = await arangoEventLogStore.GetEventsAsync("batch-1");

            // Assert
            Assert.NotNull(events);
            Assert.Empty(events);
        }

        [Fact]
        public async Task CreateBatchAsync_Success()
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            EventBatch batch = await arangoEventLogStore.CreateBatchAsync("batch-1");

            // Assert
            Assert.NotNull(batch);
            Assert.Equal("batch-1", batch.Id);

            var dbBatch = await GetDocumentObjectAsync<EventBatch>(batch.Id);
            dbBatch.Should().BeEquivalentTo(batch);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateBatchAsync_ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => arangoEventLogStore.CreateBatchAsync(id));
        }

        [Fact]
        public async Task CreateBatchAsync_WithEvents_Success()
        {
            // Arrange
            var events = new List<EventLogTuple>
            {
                new EventLogTuple(
                    new TestEvent("test-event")
                    {
                        MetaData = new EventMetaData
                        {
                            Batch = new EventBatchData
                            {
                                Id = "batch-1"
                            }
                        }
                    },   "test-stream")
                {
                    BatchId = "batch-1",
                    Id = "test-event"
                }
            };

            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            EventBatch batch = await arangoEventLogStore.CreateBatchAsync("batch-1", events);

            // Assert
            Assert.NotNull(batch);
            Assert.Equal("batch-1", batch.Id);

            var dbBatch = await GetDocumentObjectAsync<EventBatch>(batch.Id);
            dbBatch.Should().BeEquivalentTo(batch);

            var dbEvent = await GetDeserializedObjectAsync<EventLogTuple>("test-event",
                CustomJsonSubTypesConverters.TestUserProfileServiceEventConverters);

            dbEvent.Should()
                .BeEquivalentTo(events[0]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateBatchAsync_WithEvents_ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var events = new List<EventLogTuple>();
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => arangoEventLogStore.CreateBatchAsync(id, events));
        }

        [Fact]
        public async Task AddEventsAsync_Success()
        {
            // Arrange
            var batchId = "batch-1";

            var events = new List<EventTuple>
            {
                new EventTuple("test-stream", new TestEvent("test-4", batchId)),
                new EventTuple("test-stream", new TestEvent("test-5", batchId)),
                new EventTuple("test-stream", new TestEvent("test-6", batchId))
            };

            List<EventLogTuple> eventsToBeAdded = events.Select(e => new EventLogTuple(e, batchId)).ToList();

            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData(batchId, EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>()
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act
            await arangoEventLogStore.AddEventsAsync(batchId, eventsToBeAdded);

            // Arrange
            // Use build method to check result of AddEvents
            IEnumerable<EventLogTuple> dbEvents = await arangoEventLogStore.GetEventsAsync(batchId);
            Assert.Equal(6, dbEvents.Count());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AddEventsAsync_Should__ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var events = new List<EventLogTuple>();
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => arangoEventLogStore.AddEventsAsync(id, events));
        }

        [Fact]
        public async Task AddEventsAsync_Should__ThrowException_IfListOfEventsAreEmpty()
        {
            // Arrange
            var events = new List<EventLogTuple>();
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => arangoEventLogStore.AddEventsAsync("batch-1", events));
        }

        [Fact]
        public async Task AddEventsAsync_Should__ThrowException_IfListOfEventsAreNull()
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => arangoEventLogStore.AddEventsAsync("batch-1", null));
        }

        [Fact]
        public async Task AddEventsAsync_Should__ThrowException_IfBatchIdIsDifferent()
        {
            // Arrange
            var batchId = "batch-1";

            var events = new List<EventTuple>
            {
                new EventTuple("test-stream", new TestEvent("test-4", batchId)),
                new EventTuple("test-stream", new TestEvent("test-5", batchId)),
                new EventTuple("test-stream", new TestEvent("test-6", batchId))
            };

            List<EventLogTuple> eventsToBeAdded = events.Select(e => new EventLogTuple(e, batchId)).ToList();
            eventsToBeAdded[1].BatchId = "different-batch-2";

            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => arangoEventLogStore.AddEventsAsync(batchId, eventsToBeAdded));
        }

        [Fact]
        public async Task UpdateBatchAsync_Success()
        {
            // Arrange
            var batchId = "batch-1";

            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData(batchId, EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>()
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            var batchToUpdate = new EventBatch
            {
                Status = EventStatus.Error,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Id = batchId
            };

            // Act
            await arangoEventLogStore.UpdateBatchAsync(batchId, batchToUpdate);

            // Arrange
            var dbBatch = await GetDocumentObjectAsync<EventBatch>(batchId);
            dbBatch.Should().BeEquivalentTo(batchToUpdate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateBatchAsync_Should__ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var batchToUpdate = new EventBatch
            {
                Status = EventStatus.Error,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Id = "batch-1"
            };

            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => arangoEventLogStore.UpdateBatchAsync(id, batchToUpdate));
        }

        [Fact]
        public async Task UpdateBatchAsync_Should__ThrowException_IfBatchIsNull()
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => arangoEventLogStore.UpdateBatchAsync("batch-1", null));
        }

        [Fact]
        public async Task UpdateEventAsync_Success()
        {
            // Arrange
            var batchId = "batch-1";

            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData(batchId, EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>()
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            var rawEvent = new TestEvent("test-1", batchId)
            {
                Value = "new-test-value"
            };

            var eventToUpdate = new EventLogTuple(rawEvent, "stream-1")
            {
                Id = "test-1",
                UpdatedAt = DateTime.Now.ToUniversalTime(),
                BatchId = batchId
            };

            // Act
            await arangoEventLogStore.UpdateEventAsync(rawEvent.EventId, eventToUpdate);

            // Assert
            var dbEvent = await GetDeserializedObjectAsync<EventLogTuple>(
                rawEvent.EventId,
                CustomJsonSubTypesConverters.TestUserProfileServiceEventConverters);

            dbEvent.Should()
                .BeEquivalentTo(
                    eventToUpdate,
                    opt => opt.Excluding(p => p.Event)
                              .Using<DateTime>(
                            ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
                        .WhenTypeIs<DateTime>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateEventAsync_Should__ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var rawEvent = new TestEvent("test-1", "batch-1")
            {
                Value = "new-test-value"
            };

            var eventToUpdate = new EventLogTuple(rawEvent, "stream-1");

            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => arangoEventLogStore.UpdateEventAsync(id, eventToUpdate));
        }

        [Fact]
        public async Task UpdateEventAsync_Should__ThrowException_IfBatchIsNull()
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => arangoEventLogStore.UpdateEventAsync("event-1", null));
        }

        [Fact]
        public async Task AbortBatchAsync_Success()
        {
            // Arrange
            var testData = new List<EventBatchTestData>
            {
                new EventBatchTestData("batch-1", EventStatus.Executed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-1"),
                        new EventTestData("test-2"),
                        new EventTestData("test-3")
                    }
                },
                new EventBatchTestData("batch-2", EventStatus.Committed)
                {
                    EventTestData = new List<EventTestData>
                    {
                        new EventTestData("test-4"),
                        new EventTestData("test-5", EventStatus.Executed),
                        new EventTestData("test-6")
                    }
                }
            };

            var fixture = new ArangoEventLogStoreFixture(testData);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            DateTime now = DateTime.UtcNow;

            // Act
            await Task.Delay(300);

            await arangoEventLogStore.AbortBatchAsync("batch-2");

            // Assert
            var dbBatch = await GetDocumentObjectAsync<EventBatch>("batch-2");
            Assert.Equal(EventStatus.Aborted, dbBatch.Status);
            Assert.True(now < dbBatch.UpdatedAt);

            var dbEventTest4 = await GetDocumentObjectAsync<EventLogTuple>("test-4");
            Assert.True(now < dbEventTest4.UpdatedAt);

            var dbEventTest5 = await GetDocumentObjectAsync<EventLogTuple>("test-5");
            Assert.True(now > dbEventTest5.UpdatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task AbortBatchAsync_Should__ThrowException_IfInvalidBatchId(string id)
        {
            // Arrange
            var testData = new List<EventBatchTestData>();

            var fixture = new ArangoEventLogStoreFixture(testData, false);
            IFirstProjectionEventLogWriter arangoEventLogStore = await fixture.GetFirstProjectionEventLogWriter();

            // Act && Assert
            await Assert.ThrowsAsync<ArgumentException>(() => arangoEventLogStore.AbortBatchAsync(id));
        }
    }
}
