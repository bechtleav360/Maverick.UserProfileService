using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;
using UserProfileService.Projection.SecondLevel.UnitTests.Mocks;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.UnitTests.EventHandlerTests;

public class EventHandlerBaseTests
{
    private static ProjectionState ConvertHeader(StreamedEventHeader header)
    {
        return new ProjectionState
        {
            EventId = header.EventId.ToString(),
            EventName = header.EventType,
            EventNumberVersion = header.EventNumberVersion,
            StreamName = header.EventStreamId,
            ProcessedOn = header.Created,
            ProcessingStartedAt = header.Created
        };
    }

    [Fact]
    public async Task Invalid_stream_name_should_fail()
    {
        //arrange
        var repoMock =
            new Mock<ISecondLevelProjectionRepository>();

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.AddSingleton<MockUserCreatedEventHandler>();
            });

        UserCreated createdEvent =
            ResolvedEventFakers.NewUserCreated.Generate(1)
                .Single()
                .AddDefaultMetadata("not-working-stream-name");

        var sut = services.GetRequiredService<MockUserCreatedEventHandler>();

        // act & assert
        await Assert.ThrowsAsync<InvalidHeaderException>(
            () => sut.HandleEventAsync(createdEvent, createdEvent.GenerateEventHeader(0)));
    }

    [Fact]
    public async Task Null_stream_name_should_fail()
    {
        //arrange
        var repoMock =
            new Mock<ISecondLevelProjectionRepository>();

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.AddSingleton<MockUserCreatedEventHandler>();
            });

        UserCreated createdEvent =
            ResolvedEventFakers.NewUserCreated.Generate(1)
                .Single()
                .AddDefaultMetadata(null as string);

        var sut = services.GetRequiredService<MockUserCreatedEventHandler>();

        // act & assert
        await Assert.ThrowsAsync<InvalidHeaderException>(
            () => sut.HandleEventAsync(createdEvent, createdEvent.GenerateEventHeader(1)));
    }

    [Fact]
    public async Task Missing_domain_event_should_fail()
    {
        //arrange
        var repoMock =
            new Mock<ISecondLevelProjectionRepository>();

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.AddSingleton<MockUserCreatedEventHandler>();
            });

        var sut = services.GetRequiredService<MockUserCreatedEventHandler>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.HandleEventAsync(null, new StreamedEventHeader()));
    }

    [Fact]
    public async Task Missing_event_header_should_fail()
    {
        //arrange
        var repoMock =
            new Mock<ISecondLevelProjectionRepository>();

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.AddSingleton<MockUserCreatedEventHandler>();
            });

        var sut = services.GetRequiredService<MockUserCreatedEventHandler>();

        // act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleEventAsync(new UserCreated(), null));
    }

    [Fact]
    public async Task Handle_message_and_check_stored_event_id_shall_work()
    {
        //arrange
        var repoMock =
            new Mock<ISecondLevelProjectionRepository>();

        var callingSequence = new MockSequence();
        IDatabaseTransaction transaction = new MockDatabaseTransaction();

        // in this case we don't care about the event itself (here UserCreated)
        // only order is important!
        // should be: start transaction - event handling stuff - store projection state - end transaction
        repoMock.InSequence(callingSequence)
            .Setup(repo => repo.StartTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        repoMock.InSequence(callingSequence)
            .Setup(
                repo => repo.CreateProfileAsync(
                    It.IsAny<ISecondLevelProjectionProfile>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()));

        repoMock.InSequence(callingSequence)
            .Setup(
                repo => repo.SaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()));

        repoMock.InSequence(callingSequence)
            .Setup(
                repo =>
                    repo.CommitTransactionAsync(
                        It.Is<IDatabaseTransaction>(t => t == transaction),
                        It.IsAny<CancellationToken>()));

        IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
            s =>
            {
                s.AddSingleton(repoMock.Object);
                s.AddSingleton<MockUserCreatedEventHandler>();
            });

        var sut = services.GetRequiredService<MockUserCreatedEventHandler>();
        UserCreated userCreated = new UserCreated().AddDefaultMetadata("user#cool");
        StreamedEventHeader header = userCreated.GenerateEventHeader(1984);

        // act
        await sut.HandleEventAsync(userCreated, header);

        // assert
        var fluentAssertionBuilder = new ItShouldOptionsBuilder<ProjectionState>();

        fluentAssertionBuilder.Configure(
            o =>
                o.Using<DateTimeOffset>(
                        ctx =>
                            ctx.Subject.Should().BeCloseTo(ctx.Expectation, 11.Minutes()))
                    .WhenTypeIs<DateTimeOffset>());

        repoMock.Verify(
            repo => repo.StartTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.CreateProfileAsync(
                It.IsAny<ISecondLevelProjectionProfile>(),
                It.Is<IDatabaseTransaction>(t => t == transaction),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.SaveProjectionStateAsync(
                ItShould.BeEquivalentTo(
                    ConvertHeader(header),
                    fluentAssertionBuilder.Options),
                It.IsAny<IDatabaseTransaction>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        repoMock.Verify(
            repo => repo.CommitTransactionAsync(
                It.Is<IDatabaseTransaction>(t => t == transaction),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        repoMock.VerifyNoOtherCalls();
    }

    // the overridden method should not be tested here
    // the class inherits base that contains the method to be tested
    private class MockUserCreatedEventHandler : SecondLevelEventHandlerBase<UserCreated>
    {
        public MockUserCreatedEventHandler(
            ISecondLevelProjectionRepository repository,
            IMapper mapper,
            IStreamNameResolver streamNameResolver,
            IMessageInformer messageInformer,
            ILogger<MockUserCreatedEventHandler> logger)
            : base(repository, mapper, streamNameResolver, messageInformer, logger)
        {
        }

        private Task StartExecutionTestAsync(StreamedEventHeader header)
        {
            return ExecuteInsideTransactionAsync(
                (repo, transaction, ct) =>
                    repo.CreateProfileAsync(
                        new SecondLevelProjectionUser(),
                        transaction,
                        ct),
                header,
                CancellationToken.None);
        }

        /// <inheritdoc />
        protected override async Task HandleEventAsync(
            UserCreated domainEvent,
            StreamedEventHeader header,
            ObjectIdent relatedEntityIdent,
            CancellationToken cancellationToken = default)
        {
            if (header.EventStreamId != "user#cool")
            {
                return;
            }

            await StartExecutionTestAsync(header);
        }
    }
}