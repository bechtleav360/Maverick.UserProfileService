using FluentAssertions;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Marten.EventStore.Implementations;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.MartenEventStore.UnitTests.Helpers;

namespace UserProfileService.MartenEventStore.UnitTests.Implementations;

public class StreamNameResolverTests
{
    [Theory]
    [MemberData(
        nameof(StreamNameResolverTestArguments.GetStreamNamesAndReferenceObjectIdents),
        "test",
        MemberType = typeof(StreamNameResolverTestArguments))]
    public void Get_object_ident_from_stream_name_should_work(
        string streamName,
        ObjectIdent referenceValue)
    {
        // arrange
        var configMock = new Mock<IOptionsSnapshot<MartenEventStoreOptions>>();

        configMock.Setup(c => c.Value)
            .Returns(
                new MartenEventStoreOptions
                {
                    StreamNamePrefix = "test"
                });

        ILogger<StreamNameResolver> logger = new LoggerFactory().CreateLogger<StreamNameResolver>();

        IStreamNameResolver sut = new StreamNameResolver(configMock.Object, logger);

        // act
        ObjectIdent objectIdent = sut.GetObjectIdentUsingStreamName(streamName);

        // assert
        objectIdent.Should().BeEquivalentTo(referenceValue);
    }
}
