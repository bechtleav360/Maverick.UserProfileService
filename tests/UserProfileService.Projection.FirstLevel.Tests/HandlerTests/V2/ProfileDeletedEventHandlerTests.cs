using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class ProfileDeletedEventHandlerTests
    {
        private readonly FirstLevelProjectionGroup _group;
        private readonly ProfileDeletedEvent _groupDeletedEventWithoutTags;
        private readonly FirstLevelProjectionOrganization _organization;
        private readonly ProfileDeletedEvent _organizationDeletedEventWithoutTags;
        private readonly FirstLevelProjectionUser _user;
        private readonly ProfileDeletedEvent _userDeletedEventWithoutTags;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionDeletedEventHandlerTest" /> class.
        /// </summary>
        public ProfileDeletedEventHandlerTests()
        {
            _group = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();
            _user = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _organization = MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances().Single();

            _groupDeletedEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateProfileDeletedEvent(_group);

            _userDeletedEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateProfileDeletedEvent(_user);

            _organizationDeletedEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateProfileDeletedEvent(_organization);

            MockProvider.GetDefaultMock<ISagaService>();
            MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();
        }

        [Fact]
        public async Task Handler_should_work_user()
        {
            //arrange
            Mock<IDatabaseTransaction> transactionMock = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();
            Mock<IStreamNameResolver> streamResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();

            repoMock.Setup(
                    x => x.GetParentsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionContainer>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionGroup(2));
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances(2));

                        foreach (IFirstLevelProjectionContainer result in results)
                        {
                            sagaService.Setup(
                                    x => x.AddEventsAsync(
                                        It.IsAny<Guid>(),
                                        It.Is<IEnumerable<EventTuple>>(
                                            et => et.Any(
                                                t => t.TargetStream
                                                    == streamResolver.Object.GetStreamName(result.ToObjectIdent())
                                                    && t.Event.GetType() == typeof(MemberDeleted))),
                                        It.IsAny<CancellationToken>()))
                                .Verifiable();
                        }

                        return results;
                    });

            repoMock.Setup(
                    x => x.GetProfileAsync(
                        It.Is<string>(id => id == _user.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _user);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transactionMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _userDeletedEventWithoutTags,
                _userDeletedEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.DeleteProfileAsync(
                    It.Is<string>(id => id == _user.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _user.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify();
            sagaService.Verify();

            sagaService.Verify(
                s => s.ExecuteBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handler_should_work_group()
        {
            //arrange
            Mock<IDatabaseTransaction> transactionMock = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();
            Mock<IStreamNameResolver> streamResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();

            repoMock.Setup(
                    x => x.GetParentsAsync(
                        It.Is<string>(y => y == _group.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionContainer>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionGroup());
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances(2));

                        foreach (IFirstLevelProjectionProfile result in results)
                        {
                            sagaService.Setup(
                                    x => x.AddEventsAsync(
                                        It.IsAny<Guid>(),
                                        It.Is<IEnumerable<EventTuple>>(
                                            et => et.Any(
                                                t => t.TargetStream
                                                    == streamResolver.Object.GetStreamName(result.ToObjectIdent())
                                                    && t.Event.GetType() == typeof(MemberDeleted))),
                                        It.IsAny<CancellationToken>()))
                                .Verifiable();
                        }

                        return results;
                    });

            repoMock.Setup(
                    x => x.GetAllChildrenAsync(
                        It.Is<ObjectIdent>(y => y.Id == _group.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent id, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionProfile>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionGroup());
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionUser(5));

                        List<FirstLevelRelationProfile> relationMember = results.Select(
                                profile => new FirstLevelRelationProfile
                                           {
                                               Profile = profile,
                                               Relation = FirstLevelMemberRelation.DirectMember
                                           })
                            .ToList();
                        
                        foreach (IFirstLevelProjectionProfile result in results)
                        {
                            sagaService.Setup(
                                    x => x.AddEventsAsync(
                                        It.IsAny<Guid>(),
                                        It.Is<IEnumerable<EventTuple>>(
                                            et => et.Any(
                                                t => t.TargetStream
                                                    == streamResolver.Object.GetStreamName(result.ToObjectIdent())
                                                    && t.Event.GetType() == typeof(ContainerDeleted))),
                                        It.IsAny<CancellationToken>()))
                                .Verifiable();
                        }

                        return relationMember;
                    });

            repoMock.Setup(
                    x => x.GetProfileAsync(
                        It.Is<string>(id => id == _group.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _group);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transactionMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _groupDeletedEventWithoutTags,
                _groupDeletedEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.DeleteProfileAsync(
                    It.Is<string>(id => id == _group.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _group.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify();
            sagaService.Verify();

            sagaService.Verify(
                s => s.ExecuteBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handler_should_work_organization()
        {
            //arrange
            Mock<IDatabaseTransaction> transactionMock = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();
            Mock<IStreamNameResolver> streamResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();

            repoMock.Setup(
                    x => x.GetParentsAsync(
                        It.Is<string>(y => y == _organization.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionContainer>();

                        return results;
                    });

            repoMock.Setup(
                    x => x.GetAllChildrenAsync(
                        It.Is<ObjectIdent>(y => y.Id == _organization.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent id, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionProfile>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionGroup());
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionUser(5));
                        
                        foreach (IFirstLevelProjectionProfile result in results)
                        {
                            sagaService.Setup(
                                    x => x.AddEventsAsync(
                                        It.IsAny<Guid>(),
                                        It.Is<IEnumerable<EventTuple>>(
                                            et => et.Any(
                                                t => t.TargetStream
                                                    == streamResolver.Object.GetStreamName(result.ToObjectIdent())
                                                    && t.Event.GetType() == typeof(ContainerDeleted))),
                                        It.IsAny<CancellationToken>()))
                                .Verifiable();
                        }

                        return results.Select(
                                          profile => new FirstLevelRelationProfile(
                                              profile,
                                              FirstLevelMemberRelation.DirectMember))
                                      .ToList();
                    });

            repoMock.Setup(
                    x => x.GetProfileAsync(
                        It.Is<string>(id => id == _organization.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _organization);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transactionMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _organizationDeletedEventWithoutTags,
                _organizationDeletedEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.DeleteProfileAsync(
                    It.Is<string>(id => id == _organization.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _organization.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify();
            sagaService.Verify();

            sagaService.Verify(
                s => s.AddEventsAsync(
                    It.IsAny<Guid>(),
                    It.Is<IEnumerable<EventTuple>>(et => et.Any(t => t.Event.GetType() == typeof(MemberDeleted))),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            sagaService.Verify(
                s => s.ExecuteBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handler_input_output_should_work_with_group_deleted()
        {
            //arrange
            Mock<IDatabaseTransaction> transactionMock = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            var sagaService = new MockSagaService();

            repoMock.Setup(
                    x => x.GetParentsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    () => new List<IFirstLevelProjectionContainer>
                    {
                        InputSagaWorkerEventsOutputEventTuple.FunctionParentFromDeletedProfileGroup
                    });

            repoMock.Setup(
                    x => x.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    () => new List<FirstLevelRelationProfile>
                          {
                              new FirstLevelRelationProfile
                              {
                                  Profile = InputSagaWorkerEventsOutputEventTuple
                                      .UserChildToDeleteGroupProfile,
                                  Relation = FirstLevelMemberRelation.DirectMember
                              }
                          });

            repoMock.Setup(
                    x => x.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => InputSagaWorkerEventsOutputEventTuple.GroupToDeleteInProfileDeleted);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transactionMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                InputSagaWorkerEventsOutputEventTuple.ProfileDeletedEventTest,
                InputSagaWorkerEventsOutputEventTuple.ProfileDeletedEventTest.GenerateEventHeader(10));

            List<EventTuple> resultEvents = sagaService.GetDictionary().Values.First();

            resultEvents.Should()
                .BeEquivalentTo(
                    InputSagaWorkerEventsOutputEventTuple.ProfileGroupDeletedEventTuple,
                    opt => opt.Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .Excluding(p => p.Event.EventId)
                        .RespectingRuntimeTypes());
        }
    }
}
