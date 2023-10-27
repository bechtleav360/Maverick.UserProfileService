using Marten.Events;
using Moq;
using UserProfileService.MartenEventStore.UnitTests.Models;

namespace UserProfileService.MartenEventStore.UnitTests.Helpers;

internal class EventStoreMockBuilder
{
    public Mock<IEventStore> MockObject = new Mock<IEventStore>();

    public EventStoreMockBuilder ConfigAppend(string streamName, object? events = null, long version = 0)
    {
        MockObject.Setup(m => m.Append(streamName, events ?? It.IsAny<object>()))
            .Returns(
                new TestStreamAction(
                    Guid.Empty,
                    streamName,
                    StreamActionType.Append,
                    version));

        return this;
    }

    public EventStoreMockBuilder ConfigFetchStreamState(string streamName, bool isArchived = false, long version = 10)
    {
        MockObject.Setup(m => m.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new StreamState
                {
                    Key = streamName,
                    Version = version,
                    IsArchived = isArchived
                });

        return this;
    }

    public EventStoreMockBuilder ConfigFetchStreamState(string streamName, StreamState? state)
    {
        MockObject
            .Setup(m => m.FetchStreamStateAsync(streamName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        return this;
    }

    public EventStoreMockBuilder ConfigFetchStream(
        string streamName,
        object eventObject,
        long version = -1,
        DateTimeOffset? dateTime = default,
        long fromVersion = -1,
        CancellationToken token = default)
    {
        MockObject.Setup(
                es => es.FetchStreamAsync(
                    streamName,
                    version == -1 ? It.IsAny<long>() : version,
                    default ? It.IsAny<DateTimeOffset?>() : dateTime,
                    fromVersion == -1 ? It.IsAny<long>() : fromVersion,
                    default ? It.IsAny<CancellationToken>() : token))
            .ReturnsAsync(
                new List<IEvent>
                {
                    new MartenEvent
                    {
                        Data = eventObject
                    }
                });

        return this;
    }

    public Mock<IEventStore> Build()
    {
        return MockObject;
    }
}
