using System.Data;
using AutoFixture.Xunit2;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.MartenEventStore.UnitTests.Helpers;
using UserProfileService.MartenEventStore.UnitTests.Models;

namespace UserProfileService.MartenEventStore.UnitTests;

public class MartenEventStoreTests
{
    private readonly MartenEventStoreOptions _martenOptions;
    private readonly IServiceProvider _serviceProvider;

    public MartenEventStoreTests()
    {
        var serviceCollection = new ServiceCollection();

        _martenOptions = new MartenEventStoreOptions
        {
            SubscriptionName = "UserProfileServiceStream",
            ConnectionString = "Host=127.0.0.1;Username=root;Password=1;Database=test_db",
            DatabaseSchema = "UserProfileServiceSchema",
            StreamNamePrefix = "ups"
        };

        serviceCollection.AddOptions<MartenEventStoreOptions>()
            .Configure(
                o =>
                {
                    o.SubscriptionName = _martenOptions.SubscriptionName;
                    o.ConnectionString = _martenOptions.ConnectionString;
                    o.DatabaseSchema = _martenOptions.DatabaseSchema;
                    o.StreamNamePrefix = _martenOptions.StreamNamePrefix;
                });

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private IEventStorageClient GetEventStorageClient(Mock<IEventStore> eventStoreMock)
    {
        var session = new Mock<IDocumentSession>();
        session.Setup(s => s.Events).Returns(eventStoreMock.Object);

        var documentStore = new Mock<IDocumentStore>();
        documentStore.Setup(ds => ds.LightweightSession(It.IsAny<IsolationLevel>())).Returns(session.Object);

        return new Marten.EventStore.Implementations.MartenEventStore(
            documentStore.Object,
            _serviceProvider,
            new NullLogger<Marten.EventStore.Implementations.MartenEventStore>());
    }

    [Fact]
    public void GetDefaultStreamName_Should_Work()
    {
        var documentStore = new Mock<IDocumentStore>();

        var martenInternalEvenStore = new Mock<IEventStore>();

        var store = new Marten.EventStore.Implementations.MartenEventStore(
            documentStore.Object,
            _serviceProvider,
            new NullLogger<Marten.EventStore.Implementations.MartenEventStore>());

        var session = new Mock<IDocumentSession>();
        session.Setup(s => s.Events).Returns(martenInternalEvenStore.Object);

        string streamName = store.GetDefaultStreamName();

        streamName.Should().BeEquivalentTo(_martenOptions.SubscriptionName);
    }

    [Theory]
    [AutoData]
    public async Task WriteEventAsync_With_ups_event_Should_Work(UpsTestEvent @event, long version)
    {
        string? streamName = _martenOptions.SubscriptionName;

        if (streamName == null)
        {
            throw new Exception("Issue in test preparation: No stream name has been configured.");
        }

        var eventStore = new Mock<IEventStore>();

        eventStore.Setup(m => m.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StreamState
                {
                    Key = streamName,
                    Version = version - 1
                });

        eventStore.Setup(es => es.Append(streamName, It.IsAny<object>()))
            .Returns(new TestStreamAction(Guid.Empty, streamName, StreamActionType.Append, version));

        var session = new Mock<IDocumentSession>();
        session.Setup(s => s.Events).Returns(eventStore.Object);

        var documentStore = new Mock<IDocumentStore>();
        documentStore.Setup(ds => ds.LightweightSession(It.IsAny<IsolationLevel>())).Returns(session.Object);

        var store = new Marten.EventStore.Implementations.MartenEventStore(
            documentStore.Object,
            _serviceProvider,
            new NullLogger<Marten.EventStore.Implementations.MartenEventStore>());

        WriteEventResult result =
            await store.WriteEventAsync(@event, streamName, CancellationToken.None);

        eventStore.Verify(es => es.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()));

        eventStore.Verify(
            es => es.Append(
                streamName,
                It.Is<UpsTestEvent>(
                    e => e.Type == @event.Type && e.EventName == @event.EventName && e.EventId == @event.EventId)));

        result.Should()
            .BeEquivalentTo(
                new WriteEventResult
                {
                    CurrentVersion = version
                });
    }

    [Theory]
    [AutoData]
    public async Task WriteEventAsync_Write_To_Non_Existing_Stream_Should_Work(TestEvent @event, long version)
    {
        string? streamName = _martenOptions.SubscriptionName;

        if (streamName == null)
        {
            throw new Exception("Issue in test preparation: No stream name has been configured.");
        }

        var eventStore = new Mock<IEventStore>();

        eventStore.Setup(es => es.StartStream(streamName, It.IsAny<object>()))
            .Returns(new TestStreamAction(Guid.Empty, streamName, StreamActionType.Start, version));

        var session = new Mock<IDocumentSession>();
        session.Setup(s => s.Events).Returns(eventStore.Object);

        var documentStore = new Mock<IDocumentStore>();
        documentStore.Setup(ds => ds.LightweightSession(It.IsAny<IsolationLevel>())).Returns(session.Object);

        var store = new Marten.EventStore.Implementations.MartenEventStore(
            documentStore.Object,
            _serviceProvider,
            new NullLogger<Marten.EventStore.Implementations.MartenEventStore>());

        WriteEventResult result =
            await store.WriteEventAsync(@event, streamName, CancellationToken.None);

        eventStore.Verify(
            es => es.StartStream(
                streamName,
                It.Is<TestEvent>(
                    e => e.Alter == @event.Alter && e.Single == @event.Single && e.EventId == @event.EventId)));

        result.Should()
            .BeEquivalentTo(
                new WriteEventResult
                {
                    CurrentVersion = version
                });
    }

    [Theory]
    [AutoData]
    public async Task WriteEventAsync_Write_To_existing_Stream_Should_Work(TestEvent @event, long version)
    {
        string? streamName = _martenOptions.SubscriptionName;

        if (streamName == null)
        {
            throw new Exception("Issue in test preparation: No stream name has been configured.");
        }

        var eventStore = new Mock<IEventStore>();

        eventStore.Setup(m => m.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StreamState
                {
                    Key = streamName,
                    Version = version - 1
                });

        eventStore.Setup(es => es.Append(streamName, It.IsAny<object>()))
            .Returns(new TestStreamAction(Guid.Empty, streamName, StreamActionType.Append, version));

        var session = new Mock<IDocumentSession>();
        session.Setup(s => s.Events).Returns(eventStore.Object);

        var documentStore = new Mock<IDocumentStore>();
        documentStore.Setup(ds => ds.LightweightSession(It.IsAny<IsolationLevel>())).Returns(session.Object);

        var store = new Marten.EventStore.Implementations.MartenEventStore(
            documentStore.Object,
            _serviceProvider,
            new NullLogger<Marten.EventStore.Implementations.MartenEventStore>());

        WriteEventResult result =
            await store.WriteEventAsync(@event, streamName, CancellationToken.None);

        eventStore.Verify(es => es.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()));

        eventStore.Verify(
            es => es.Append(
                streamName,
                It.Is<TestEvent>(
                    e => e.Alter == @event.Alter && e.Single == @event.Single && e.EventId == @event.EventId)));

        result.Should()
            .BeEquivalentTo(
                new WriteEventResult
                {
                    CurrentVersion = version
                });
    }

    [Theory]
    [AutoData]
    public async Task GetLastStreamFromEvent_should_work(UpsTestEvent @event, long version)
    {
        string? streamName = _martenOptions.SubscriptionName;

        if (streamName == null)
        {
            throw new Exception("Issue in test preparation: No stream name has been configured.");
        }

        Mock<IEventStore> eventStoreMock = new EventStoreMockBuilder()
            .ConfigFetchStreamState(streamName, false, version)
            .ConfigFetchStream(
                streamName,
                @event,
                fromVersion: version,
                token: CancellationToken.None)
            .Build();

        IEventStorageClient client = GetEventStorageClient(eventStoreMock);

        var lastEvent = await client.GetLastEventFromStreamAsync<UpsTestEvent>(streamName, CancellationToken.None);

        lastEvent.Should().BeEquivalentTo(@event);
    }

    [Theory]
    [AutoData]
    public async Task GetLastStreamFromEvent_should_throw_by_unknown_stream_Name(UpsTestEvent @event, long version)
    {
        string? streamName = _martenOptions.SubscriptionName;
        
        if (streamName == null)
        {
            throw new Exception("Issue in test preparation: No stream name has been configured.");
        }

        Mock<IEventStore> eventStoreMock =
            new EventStoreMockBuilder().ConfigFetchStream(streamName, @event, version).Build();

        IEventStorageClient client = GetEventStorageClient(eventStoreMock);

        await Assert.ThrowsAsync<EventStreamNotFoundException>(
            async () => await client.GetLastEventFromStreamAsync<UpsTestEvent>(streamName, CancellationToken.None));
    }

    [Theory]
    [AutoData]
    public async Task SoftDeleteStreamAsync_should_work(string streamName)
    {
        Mock<IEventStore> eventStoreMock = new EventStoreMockBuilder().ConfigFetchStreamState(streamName).Build();
        IEventStorageClient client = GetEventStorageClient(eventStoreMock);

        await client.SoftDeleteStreamAsync(streamName);

        eventStoreMock.Verify(e => e.ArchiveStream(streamName));
    }

    [Theory]
    [AutoData]
    public async Task SoftDeleteStreamAsync_should_throw_when_stream_deleted(string streamName)
    {
        Mock<IEventStore> eventStoreMock =
            new EventStoreMockBuilder().ConfigFetchStreamState(streamName, true).Build();

        IEventStorageClient client = GetEventStorageClient(eventStoreMock);

        await Assert.ThrowsAsync<AccessDeletedStreamException>(
            async () => await client.SoftDeleteStreamAsync(streamName));
    }

    [Theory]
    [AutoData]
    public async Task SoftDeleteStreamAsync_should_throw_when_stream_not_exits(string streamName)
    {
        Mock<IEventStore> eventStoreMock =
            new EventStoreMockBuilder().ConfigFetchStreamState(streamName, null).Build();

        IEventStorageClient client = GetEventStorageClient(eventStoreMock);

        await Assert.ThrowsAsync<EventStreamNotFoundException>(
            async () => await client.SoftDeleteStreamAsync(streamName));
    }
}
