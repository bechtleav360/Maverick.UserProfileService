using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    /// <summary>
    ///     Tests for <see cref="OrganizationCreatedFirstLevelEventHandler" />
    /// </summary>
    public class OrganizationCreatedHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly List<ConditionObjectIdent> _conditionalMembers;
        private readonly OrganizationCreatedEvent _createdEventWithoutTagsWithMembers;
        private readonly OrganizationCreatedEvent _createdEventWithoutTagsWithoutMembers;
        private readonly OrganizationCreatedEvent _createdEventWithTagsAndMembers;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;
        private readonly FirstLevelProjectionOrganization _organization;
        private readonly List<TagAssignment> _tagsAssignments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrganizationCreatedHandlerTest" /> class.
        /// </summary>
        public OrganizationCreatedHandlerTest()
        {
            _conditionalMembers = MockDataGenerator.GenerateConditionObjectIdents(10, ObjectType.Organization);
            _organization = MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances().Single();
            _organization.UpdatedAt = _organization.CreatedAt;
            _tagsAssignments = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true);

            _createdEventWithoutTagsWithoutMembers =
                MockedSagaWorkerEventsBuilder.CreateOrganizationCreatedEvent(_organization);

            _createdEventWithTagsAndMembers =
                MockedSagaWorkerEventsBuilder.CreateOrganizationCreatedEvent(
                    _organization,
                    _tagsAssignments.ToArray(),
                    _conditionalMembers);

            _createdEventWithoutTagsWithMembers =
                MockedSagaWorkerEventsBuilder.CreateOrganizationCreatedEvent(
                    _organization,
                    null,
                    _conditionalMembers);

            _mockSagaService = MockProvider.GetDefaultMock<ISagaService>();
            _mockCreator = MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();
        }

        [Fact]
        public async Task Handler_should_work_without_tags_and_members()
        {
            // arrange
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            repoMock.Setup(
                    m => m.OrganizationExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                _createdEventWithoutTagsWithoutMembers,
                _createdEventWithoutTagsWithoutMembers.GenerateEventHeader(10),
                CancellationToken.None);

            // assert
            repoMock.Verify(
                repo => repo.CreateProfileAsync(
                    ItShould.BeEquivalentTo(
                        _organization,
                        opt => opt.Excluding(g => g.SynchronizedAt).Excluding(g => g.IsMarkedForDeletion)),
                    It.Is<IDatabaseTransaction>(
                        t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContainerType>(),
                    It.IsAny<string>(),
                    It.IsAny<RangeCondition[]>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            sagaService.Verify(
                s => s.CreateBatchAsync(
                    CancellationToken.None,
                    It.IsAny<EventTuple[]>()),
                Times.Once);

            sagaService.Verify(
                s => s.ExecuteBatchAsync(
                    It.Is<Guid>(guid => guid == MockProvider.BatchGuid),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Handler_should_throw_when_organization_already_exist()
        {
            // arrange
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            repoMock.Setup(
                    m => m.OrganizationExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            // act

            await Assert.ThrowsAsync<AlreadyExistsException>(async () => await sut.HandleEventAsync(
                _createdEventWithoutTagsWithoutMembers,
                _createdEventWithoutTagsWithoutMembers.GenerateEventHeader(10),
                CancellationToken.None));

            // assert
            repoMock.Verify(
                repo => repo.CreateProfileAsync(
                    ItShould.BeEquivalentTo(
                        _organization,
                        opt => opt.Excluding(g => g.SynchronizedAt).Excluding(g => g.IsMarkedForDeletion)),
                    It.Is<IDatabaseTransaction>(
                        t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContainerType>(),
                    It.IsAny<string>(),
                    It.IsAny<RangeCondition[]>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            sagaService.Verify(
                s => s.CreateBatchAsync(
                    CancellationToken.None,
                    It.IsAny<EventTuple[]>()),
                Times.Never);

            sagaService.Verify(
                s => s.ExecuteBatchAsync(
                    It.Is<Guid>(guid => guid == MockProvider.BatchGuid),
                    CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Handler_should_work_with_member_without_tags()
        {
            // arrange
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            repoMock.Setup(
                    m => m.OrganizationExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            repoMock
                .Setup(
                    repo => repo.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(
                    (string tagId, IDatabaseTransaction _, CancellationToken __)
                        =>
                    {
                        TagAssignment t = _tagsAssignments
                            .Single(t => t.TagId == tagId);

                        return new FirstLevelProjectionTag
                        {
                            Id = t.TagId,
                            Name = t.TagId
                        };
                    });

            repoMock
                .Setup(
                    repo => repo.GetProfileAsync(
                        It.IsAny<string>(),
                        It.Is<IDatabaseTransaction>(
                            t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                        CancellationToken.None))
                .ReturnsAsync(_organization);

            var calls = 0;

            repoMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.Is<IDatabaseTransaction>(
                            t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                        CancellationToken.None))
                .ReturnsAsync(
                    () => calls++ > 0
                        ? new List<FirstLevelRelationProfile>()
                        : new List<FirstLevelRelationProfile>
                          {
                              new FirstLevelRelationProfile
                              {
                                  Profile = MockDataGenerator
                                      .GenerateFirstLevelProjectionOrganizationWithId(
                                          "oe-1"),
                                  Relation = FirstLevelMemberRelation.DirectMember
                              }
                          });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                _createdEventWithoutTagsWithMembers,
                _createdEventWithoutTagsWithMembers.GenerateEventHeader(10),
                CancellationToken.None);

            // assert
            repoMock.Verify(
                repo => repo.CreateProfileAsync(
                    ItShould.BeEquivalentTo(
                        _organization,
                        opt => opt.Excluding(g => g.SynchronizedAt).Excluding(g => g.IsMarkedForDeletion)),
                    It.Is<IDatabaseTransaction>(
                        t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContainerType>(),
                    It.IsAny<string>(),
                    It.IsAny<RangeCondition[]>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(10));

            sagaService.Verify(
                s => s.CreateBatchAsync(
                    CancellationToken.None,
                    It.IsAny<EventTuple[]>()),
                Times.Once);

            sagaService.Verify(
                s => s.ExecuteBatchAsync(
                    It.Is<Guid>(guid => guid == MockProvider.BatchGuid),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Handler_should_work_with_member_with_tags()
        {
            // arrange
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            repoMock.Setup(
                    m => m.OrganizationExistAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            repoMock
                .Setup(
                    repo => repo.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(
                    (string tagId, IDatabaseTransaction _, CancellationToken __)
                        =>
                    {
                        TagAssignment t = _tagsAssignments
                            .Single(t => t.TagId == tagId);

                        return new FirstLevelProjectionTag
                        {
                            Id = t.TagId,
                            Name = t.TagId
                        };
                    });

            repoMock
                .Setup(
                    repo => repo.GetProfileAsync(
                        It.IsAny<string>(),
                        It.Is<IDatabaseTransaction>(
                            t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                        CancellationToken.None))
                .ReturnsAsync(_organization);

            var calls = 0;

            repoMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.Is<IDatabaseTransaction>(
                            t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                        CancellationToken.None))
                .ReturnsAsync(
                    () => calls++ > 0
                        ? new List<FirstLevelRelationProfile>()
                        : new List<FirstLevelRelationProfile>
                          {
                              new FirstLevelRelationProfile
                              {
                                  Profile = MockDataGenerator
                                      .GenerateFirstLevelProjectionOrganizationWithId(
                                          "oe-1"),
                                  Relation = FirstLevelMemberRelation.DirectMember
                              }
                          });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                _createdEventWithTagsAndMembers,
                _createdEventWithTagsAndMembers.GenerateEventHeader(10),
                CancellationToken.None);

            // assert
            repoMock.Verify(
                repo => repo.CreateProfileAsync(
                    ItShould.BeEquivalentTo(
                        _organization,
                        opt => opt.Excluding(g => g.SynchronizedAt).Excluding(g => g.IsMarkedForDeletion)),
                    It.Is<IDatabaseTransaction>(
                        t => ((MockDatabaseTransaction)t).Id == ((MockDatabaseTransaction)transaction).Id),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    CancellationToken.None),
                // Amount of GetTag calls:
                // * NumberTagAssignments (it's a number) per organization, all of them inherited,
                //  * n times for the new organization (n = NumberTagAssignments),
                //  * 10 existing organizations to be assigned, each of them n tags (n = NumberTagAssignments)
                //  * n times for one grand-child, because one of the new children has a member too (n = NumberTagAssignments)
                Times.Exactly(NumberTagAssignments * (1 + 10 + 1)));

            repoMock.Verify(
                repo => repo.AddTagToProfileAsync(
                    It.IsAny<FirstLevelProjectionTagAssignment>(),
                    It.IsAny<string>(),
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    CancellationToken.None),
                // Amount of AddTagToProfileAsync calls:
                // like above (GetTag)
                Times.Exactly(NumberTagAssignments * 12));

            repoMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContainerType>(),
                    It.IsAny<string>(),
                    It.IsAny<RangeCondition[]>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(10));

            sagaService.Verify(
                s => s.CreateBatchAsync(
                    CancellationToken.None,
                    It.IsAny<EventTuple[]>()),
                Times.Once);

            sagaService.Verify(
                s => s.ExecuteBatchAsync(
                    It.Is<Guid>(guid => guid == MockProvider.BatchGuid),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_event()
        {
            //arrange
            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(null, null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_transaction()
        {
            //arrange
            var repoMock = new Mock<IFirstLevelProjectionRepository>();
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _createdEventWithTagsAndMembers,
                    _createdEventWithTagsAndMembers.GenerateEventHeader(10),
                    CancellationToken.None));
        }
    }
}
