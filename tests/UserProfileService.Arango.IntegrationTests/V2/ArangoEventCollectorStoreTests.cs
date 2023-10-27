using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Query;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.EventCollector.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2
{
    [Collection(nameof(DatabaseCollection))]
    public class ArangoEventCollectorStoreTests : ArangoDbTestBase
    {
        private readonly DatabaseFixture _fixture;
        private readonly ModelBuilderOptions _modelInfo;

        public ArangoEventCollectorStoreTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _modelInfo = _fixture.DefaultModelBuilderOptionsEventCollectorStore;
        }

        protected async Task<IReadOnlyList<TType>> GetDocumentObjectsAsync<TType>(
            string aqlQuery,
            bool throwExceptions,
            params string[] bindParameterKeyAndValues)
        {
            var bindParameters = new Dictionary<string, object>();

            for (var i = 0; i < bindParameterKeyAndValues.Length; i += 2)
            {
                bindParameters.Add(
                    bindParameterKeyAndValues[i],
                    bindParameterKeyAndValues[i + 1]);
            }

            IArangoDbClient client = GetArangoClient();

            MultiApiResponse<TType> response = await client.ExecuteQueryWithCursorOptionsAsync<TType>(
                new CreateCursorBody
                {
                    Query = aqlQuery,
                    BindVars = bindParameters
                });

            if (response.Error && throwExceptions)
            {
                throw new AggregateException(
                    "Oops. Something went wrong during test.",
                    response.Responses.Select(r => r.Exception));
            }

            return response.QueryResult;
        }

        [Fact]
        public async Task SaveEventData_Success()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();
            var testData = new TestClass();

            var eventData = new EventData
            {
                CollectingId = Guid.NewGuid(),
                Data = JsonSerializer.Serialize(testData),
                Host = "host",
                ErrorOccurred = true
            };

            string collection = _modelInfo.GetCollectionName<EventData>();

            // Act
            int result = await collectorStore.SaveEventDataAsync(eventData);

            // Assert
            Assert.Equal(1, result);

            IReadOnlyList<EventData> repoEventDataList =
                await GetDocumentObjectsAsync<EventData>(
                    $"for x in {collection} filter x.{nameof(EventData.CollectingId)} == '{eventData.CollectingId}' return x",
                    true);

            EventData repoEventData = Assert.Single(repoEventDataList);

            eventData.Should().BeEquivalentTo(repoEventData);
        }

        [Fact]
        public async Task SaveEventData_Should_Throw_ArgumentNullException_IfDataIsNull()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => collectorStore.SaveEventDataAsync(null));
        }

        [Fact]
        public async Task GetEventData_Success()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();
            var testData = new TestClass();
            var processId = Guid.NewGuid();

            var eventData = new EventData
            {
                CollectingId = processId,
                Data = JsonSerializer.Serialize(testData),
                Host = "host",
                ErrorOccurred = true
            };

            await collectorStore.SaveEventDataAsync(eventData);
            await collectorStore.SaveEventDataAsync(eventData);

            // Act
            ICollection<EventData> result = await collectorStore.GetEventData(processId.ToString());

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => eventData.Should().BeEquivalentTo(r));
        }

        [Fact]
        public async Task GetEventData_Should_Throw_ArgumentNullException_IfProcessIdIsNull()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => collectorStore.GetEventData(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetEventData_Should_Throw_ArgumentException_IfProcessIdIsEmptyOrWhitespace(
            string processId)
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => collectorStore.GetEventData(processId));
        }

        [Fact]
        public async Task CountEventData_Success()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();
            var testData = new TestClass();
            var processId = Guid.NewGuid();

            var eventData = new EventData
            {
                CollectingId = processId,
                Data = JsonSerializer.Serialize(testData)
            };

            await collectorStore.SaveEventDataAsync(eventData);
            await collectorStore.SaveEventDataAsync(eventData);

            // Act
            int result = await collectorStore.GetCountOfEventDataAsync(processId.ToString());

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task CountEventData_Should_Throw_ArgumentNullException_IfProcessIdIsNull()
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => collectorStore.GetCountOfEventDataAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CountEventData_Should_Throw_ArgumentException_IfProcessIdIsEmptyOrWhitespace(
            string processId)
        {
            // Arrange
            IEventCollectorStore collectorStore = await _fixture.GetEventCollectorStoreAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => collectorStore.GetCountOfEventDataAsync(processId));
        }

        private class TestClass
        {
            public string Name { get; set; } = "Test";

            public ICollection<string> Results { get; set; } = new List<string>
            {
                "test 1",
                "test 2"
            };
        }
    }
}
