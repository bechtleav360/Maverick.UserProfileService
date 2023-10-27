using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Common;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using EventInitiatorResolved = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using InitiatorType = UserProfileService.EventSourcing.Abstractions.Models;
using EventInitiatorStore = UserProfileService.EventSourcing.Abstractions.Models.EventInitiator;

namespace UserProfileService.Projection.FirstLevel.Tests.Mocks
{
    /// <summary>
    ///     Provides Mocks for interfaces with a default configuration
    /// </summary>
    internal static class MockProvider
    {
        private static readonly Dictionary<Type, Func<MockBehavior, Mock>> _knownMocks =
            new Dictionary<Type, Func<MockBehavior, Mock>>();

        private static readonly IMapper _mapper;
        internal static Guid BatchGuid => Guid.Parse("12345678-1234-1234-1234-123456789012");

        static MockProvider()
        {
            _knownMocks.Add(typeof(ISagaService), GetDefaultSagaServiceMock);
            _knownMocks.Add(typeof(IFirstLevelProjectionRepository), GetDefaultFirstLevelProjectionRepositoryMock);
            _knownMocks.Add(typeof(IDatabaseTransaction), GetDefaultDatabaseTransactionMock);
            _knownMocks.Add(typeof(IFirstLevelEventTupleCreator), GetDefaultFirstLevelEventTupleCreatorMock);
            _knownMocks.Add(typeof(IStreamNameResolver), GetDefaultStreamNameResolverMock);
            _knownMocks.Add(typeof(ITemporaryAssignmentsExecutor), GetDefaultFirstLevelTemporaryAssignmentExecutorMock);
            _mapper = FirstLevelHandlerTestsPreparationHelper.GetMapper();
        }

        internal static Mock<TInterfaceType> GetDefaultMock<TInterfaceType>(
            MockBehavior mockBehavior = MockBehavior.Default)
            where TInterfaceType : class
        {
            if (_knownMocks.ContainsKey(typeof(TInterfaceType)))
            {
                return _knownMocks[typeof(TInterfaceType)].Invoke(mockBehavior).As<TInterfaceType>();
            }

            return new Mock<TInterfaceType>();
        }

        internal static IDatabaseTransaction GetDefaultTransactionMock()
        {
            return new MockDatabaseTransaction("123");
        }

        internal static Mock GetDefaultFirstLevelEventTupleCreatorMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<IFirstLevelEventTupleCreator>(mockBehavior);
            IStreamNameResolver streamNameResolver = GetDefaultMock<IStreamNameResolver>().Object;

            mock.Setup(
                    opt => opt.CreateEvent(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IUserProfileServiceEvent>(),
                        It.IsAny<InitiatorType.IDomainEvent>()))
                .Returns(
                    (
                        ObjectIdent objectIdent,
                        IUserProfileServiceEvent userEvent,
                        InitiatorType.IDomainEvent domainEvent) =>
                    {
                        userEvent.MetaData.CorrelationId = domainEvent?.CorrelationId ?? "correlation-id";

                        userEvent.MetaData.Initiator =
                            domainEvent != null
                                ? _mapper.Map<EventInitiatorResolved>(domainEvent.Initiator)
                                : EventInitiatorResolved.SystemInitiator;

                        userEvent.MetaData.RelatedEntityId = streamNameResolver.GetStreamName(objectIdent);
                        userEvent.MetaData.HasToBeInverted = false;
                        userEvent.MetaData.Timestamp = DateTime.UtcNow;
                        userEvent.MetaData.VersionInformation = 1;
                        userEvent.MetaData.ProcessId = domainEvent?.RequestSagaId;
                        userEvent.EventId = Guid.NewGuid().ToString();

                        return new EventTuple(streamNameResolver.GetStreamName(objectIdent), userEvent);
                    });

            mock.Setup(
                    opt => opt.CreateEvents(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IEnumerable<IUserProfileServiceEvent>>(),
                        It.IsAny<InitiatorType.IDomainEvent>()))
                .Returns(
                    (
                        ObjectIdent objectIdent,
                        IEnumerable<IUserProfileServiceEvent> uspEvents,
                        InitiatorType.IDomainEvent domainEvent) =>
                    {
                        var eventTuple = new List<EventTuple>();

                        EventInitiatorStore initiator = domainEvent?.Initiator
                            ?? new EventInitiatorStore
                               {
                                   Id = Guid.NewGuid().ToString(),
                                   Type = InitiatorType.InitiatorType.System
                               };

                        foreach (IUserProfileServiceEvent uspEvent in uspEvents)
                        {
                            uspEvent.MetaData.CorrelationId = domainEvent?.CorrelationId ?? Activity.Current?.Id;
                            uspEvent.MetaData.Initiator = _mapper.Map<EventInitiatorResolved>(initiator);
                            uspEvent.MetaData.RelatedEntityId = streamNameResolver.GetStreamName(objectIdent);
                            uspEvent.MetaData.HasToBeInverted = false;
                            uspEvent.MetaData.Timestamp = DateTime.UtcNow;
                            uspEvent.MetaData.VersionInformation = 1;
                            uspEvent.MetaData.ProcessId = domainEvent?.RequestSagaId ?? Guid.NewGuid().ToString();
                            uspEvent.EventId = Guid.NewGuid().ToString();
                            eventTuple.Add(new EventTuple(streamNameResolver.GetStreamName(objectIdent), uspEvent));
                        }

                        return eventTuple.ToArray();
                    });

            return mock;
        }

        private static Mock GetDefaultDatabaseTransactionMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<IDatabaseTransaction>(mockBehavior);
            mock.SetReturnsDefault<IDatabaseTransaction>(new MockDatabaseTransaction("123"));

            return mock;
        }

        private static Mock GetDefaultSagaServiceMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<ISagaService>(mockBehavior);
            mock.SetReturnsDefault(new MockSagaService());

#region CreateBatchAsync Setup

            mock.Setup(
                    x => x.CreateBatchAsync(
                        It.IsAny<CancellationToken>(),
                        It.Is<EventTuple[]>(et => et == null || !et.Any())))
                .Throws<ArgumentException>();

            mock.Setup(x => x.CreateBatchAsync(It.IsAny<CancellationToken>(), It.IsAny<EventTuple[]>()))
                .ReturnsAsync(
                    (CancellationToken t, EventTuple[] et) =>
                    {
                        Guid id = BatchGuid;

                        mock.Setup(
                                y => y.AddEventsAsync(
                                    It.Is<Guid>(v => v == id),
                                    It.IsAny<IEnumerable<EventTuple>>(),
                                    It.IsAny<CancellationToken>()))
                            .Verifiable();

                        mock.Setup(
                            y => y.ExecuteBatchAsync(
                                It.Is<Guid>(v => v == id),
                                It.IsAny<CancellationToken>()));

                        mock.Setup(
                            y => y.AbortBatchAsync(
                                It.Is<Guid>(v => v == id),
                                It.IsAny<CancellationToken>()));

                        mock.Object.AddEventsAsync(id, et, t);

                        return id;
                    });

#endregion

#region AddEventsAsync Setup

            mock.Setup(
                    x => x.AddEventsAsync(
                        It.Is<Guid>(id => id == Guid.Empty),
                        It.IsAny<IEnumerable<EventTuple>>(),
                        It.IsAny<CancellationToken>()))
                .Throws<ArgumentException>();

            mock.Setup(
                    x => x.AddEventsAsync(
                        It.IsAny<Guid>(),
                        It.Is<IEnumerable<EventTuple>>(et => et == null),
                        It.IsAny<CancellationToken>()))
                .Throws<ArgumentNullException>();

            mock.Setup(
                    x => x.AddEventsAsync(
                        It.IsAny<Guid>(),
                        It.Is<IEnumerable<EventTuple>>(et => !et.Any()),
                        It.IsAny<CancellationToken>()))
                .Throws<ArgumentException>();

            mock.Setup(
                x => x.AddEventsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<EventTuple>>(),
                    It.IsAny<CancellationToken>()));

#endregion

            return mock;
        }

        private static Mock GetDefaultFirstLevelProjectionRepositoryMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>(mockBehavior);
            IDatabaseTransaction transaction = GetDefaultTransactionMock();

            mock.Setup(x => x.StartTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((CancellationToken c) => transaction);

            mock.Setup(
                    x => x.CommitTransactionAsync(
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mock.Setup(
                    x => x.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mock.Setup(
                    x => x.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent ident, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        List<ObjectIdentPath> results = MockDataGenerator.GenerateObjectIdentInstances(5)
                                                                         .Select(r => new ObjectIdentPath(r.Id, r.Type))
                                                                         .ToList();

                        return results;
                    });

            mock.Setup(
                    x => x.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent ident, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionProfile>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionUser(5));
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances(2));

                        // TODO add groups?
                        return results.Select(
                                          profile => new FirstLevelRelationProfile(
                                              profile,
                                              FirstLevelMemberRelation.DirectMember))
                                      .ToList();
                    });

            return mock;
        }

        private static Mock GetDefaultStreamNameResolverMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<IStreamNameResolver>(mockBehavior);
            var streamNamePrefix = "Mock";

            mock.Setup(x => x.GetStreamName(It.IsAny<ObjectIdent>()))
                .Returns((ObjectIdent oi) => $"{streamNamePrefix}-{oi.Type}-{oi.Id}");

            return mock;
        }

        private static Mock GetDefaultFirstLevelTemporaryAssignmentExecutorMock(MockBehavior mockBehavior)
        {
            var mock = new Mock<ITemporaryAssignmentsExecutor>(mockBehavior);

            mock.Setup(
                    o => o
                        .CheckTemporaryAssignmentsAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken ct) =>
                    {
                        ct.ThrowIfCancellationRequested();

                        return Task.CompletedTask;
                    });

            return mock;
        }
    }
}
