using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Implementation;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using EventInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using EventStoreInitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;
using EventStoreEventInitiator = UserProfileService.EventSourcing.Abstractions.Models.EventInitiator;
using InitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.ImplementationTests
{
    public class FirstLevelEventCreatorTests
    {
        private const string CorrelationId = "correlation-id-1";
        private const string ProcessId = "process-no-1";

        private static EventInitiator DefaultInitiator =>
            new EventInitiator
            {
                Id = "S-123-12345-123456",
                Type = InitiatorType.User
            };

        private static EventStoreEventInitiator DefaultEventStoreInitiator =>
            new EventStoreEventInitiator
            {
                Id = "S-123-12345-123456",
                Type = EventStoreInitiatorType.User
            };

        private static IDomainEvent GenerateDomainEventMock()
        {
            var domainEventMock = new Mock<IDomainEvent>();

            domainEventMock.Setup(e => e.CorrelationId)
                .Returns(CorrelationId);

            domainEventMock.Setup(e => e.Initiator)
                .Returns(DefaultEventStoreInitiator);

            domainEventMock.Setup(e => e.RequestSagaId)
                .Returns(ProcessId);

            return domainEventMock.Object;
        }

        private static IUserProfileServiceEvent AddMetadata(
            IUserProfileServiceEvent upsEvent,
            string relatedEntityId,
            Activity activity = null)
        {
            upsEvent.MetaData.ProcessId = activity == null
                ? ProcessId
                : null;

            upsEvent.MetaData.CorrelationId = activity?.Id ?? CorrelationId;

            upsEvent.MetaData.Initiator = activity == null
                ? DefaultInitiator
                : EventInitiator.SystemInitiator;

            upsEvent.MetaData.RelatedEntityId = relatedEntityId;
            upsEvent.MetaData.VersionInformation = 1;

            return upsEvent;
        }

        private static IUserProfileServiceEvent CloneJson(IUserProfileServiceEvent upsEvent)
        {
            return upsEvent switch
            {
                OrganizationCreated orgCreated => orgCreated.CloneJson(),
                GroupCreated groupCreated => groupCreated.CloneJson(),
                _ => throw new NotSupportedException("This type is not supported by this method.")
            };
        }

        private static IUserProfileServiceEvent GetNewOrgCreatedEvent()
        {
            return ResolvedEventFakers
                .NewOrganizationCreated
                .RuleFor(e => e.CreatedAt, faker => faker.Date.Past())
                .Generate(1)
                .Single();
        }

        private static IUserProfileServiceEvent GetNewGroupCreatedEvent()
        {
            return ResolvedEventFakers
                .NewGroupCreated
                .RuleFor(e => e.CreatedAt, faker => faker.Date.Past())
                .Generate(1)
                .Single();
        }

        [Fact]
        public void Create_single_event_tuple_should_work()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            var upsEvent = (OrganizationCreated)GetNewOrgCreatedEvent();
            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            var expected = new EventTuple(
                services.GetRequiredService<IStreamNameResolver>()
                    .GetStreamName(resourceId),
                AddMetadata(
                    upsEvent.CloneJson(),
                    services
                        .GetRequiredService<IStreamNameResolver>()
                        .GetStreamName(resourceId)));

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act
            EventTuple newTuple = sut.CreateEvent(
                resourceId,
                upsEvent,
                domainEvent);

            // assert
            newTuple.Should()
                .BeEquivalentTo(
                    expected,
                    o => o.Excluding(t => t.Event.EventId));

            newTuple.Event.EventId.Should().NotBe(expected.Event.EventId);
        }

        [Fact]
        public void Create_several_event_tuples_should_work()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            var upsEvents = new List<IUserProfileServiceEvent>
            {
                GetNewOrgCreatedEvent(),
                GetNewGroupCreatedEvent(),
                GetNewOrgCreatedEvent()
            };

            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            IEnumerable<EventTuple> expected = upsEvents
                .Select(
                    upsEvent =>
                        new EventTuple(
                            services.GetRequiredService<IStreamNameResolver>()
                                .GetStreamName(resourceId),
                            AddMetadata(
                                CloneJson(upsEvent),
                                services
                                    .GetRequiredService<IStreamNameResolver>()
                                    .GetStreamName(resourceId))));

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act
            IEnumerable<EventTuple> newTuples = sut.CreateEvents(
                resourceId,
                upsEvents,
                domainEvent);

            // assert
            newTuples.Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public void Create_single_event_tuple_without_domain_event_should_work()
        {
            // arrange
            var activity =
                new Activity(nameof(Create_single_event_tuple_without_domain_event_should_work));

            activity.Start();
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            var upsEvent = (OrganizationCreated)GetNewOrgCreatedEvent();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            var expected = new EventTuple(
                services.GetRequiredService<IStreamNameResolver>()
                    .GetStreamName(resourceId),
                AddMetadata(
                    CloneJson(upsEvent),
                    services
                        .GetRequiredService<IStreamNameResolver>()
                        .GetStreamName(resourceId),
                    activity));

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act
            EventTuple newTuple = sut.CreateEvent(
                resourceId,
                upsEvent);

            activity.Stop();

            // assert
            newTuple.Should()
                .BeEquivalentTo(
                    expected,
                    o => o.Excluding(t => t.Event.EventId));

            newTuple.Event.EventId.Should().NotBe(expected.Event.EventId);
        }

        [Fact]
        public void Create_several_event_tuples_without_domain_event_should_work()
        {
            // arrange
            var activity =
                new Activity(nameof(Create_several_event_tuples_without_domain_event_should_work));

            activity.Start();
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            var upsEvents = new List<IUserProfileServiceEvent>
            {
                GetNewGroupCreatedEvent(),
                GetNewGroupCreatedEvent(),
                GetNewOrgCreatedEvent(),
                GetNewGroupCreatedEvent()
            };

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            IEnumerable<EventTuple> expected = upsEvents
                .Select(
                    upsEvent =>
                        new EventTuple(
                            services.GetRequiredService<IStreamNameResolver>()
                                .GetStreamName(resourceId),
                            AddMetadata(
                                CloneJson(upsEvent),
                                services
                                    .GetRequiredService<IStreamNameResolver>()
                                    .GetStreamName(resourceId),
                                activity)));

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act
            IEnumerable<EventTuple> newTuples = sut.CreateEvents(
                resourceId,
                upsEvents);

            activity.Stop();

            // assert
            newTuples.Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public void Create_single_event_tuple_missing_resourceId_should_fail()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            IUserProfileServiceEvent upsEvent = GetNewOrgCreatedEvent();
            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () =>
                    sut.CreateEvent(
                        null,
                        upsEvent,
                        domainEvent));
        }

        [Fact]
        public void Create_single_event_tuple_missing_upsEvent_should_fail()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () => sut.CreateEvent(
                    resourceId,
                    null,
                    domainEvent));
        }

        [Fact]
        public void Create_several_event_tuples_missing_resourceId_should_fail()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            var upsEvents = new List<IUserProfileServiceEvent>
            {
                GetNewGroupCreatedEvent()
            };

            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () =>
                    sut.CreateEvents(
                        null,
                        upsEvents,
                        domainEvent));
        }

        [Fact]
        public void Create_several_event_tuples_missing_upsEvent_should_fail()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();

            IDomainEvent domainEvent = GenerateDomainEventMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => s
                    .AddSingleton(streamNameResolverMock)
                    .AddSingleton<IFirstLevelEventTupleCreator, FirstLevelEventCreator>());

            var resourceId = new ObjectIdent("test-1", ObjectType.Organization);

            var sut = services.GetRequiredService<IFirstLevelEventTupleCreator>();

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () => sut.CreateEvents(
                    resourceId,
                    null,
                    domainEvent));
        }
    }
}
