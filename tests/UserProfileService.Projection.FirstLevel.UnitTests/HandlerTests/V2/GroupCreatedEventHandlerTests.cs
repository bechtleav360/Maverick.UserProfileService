using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Comparer;
using UserProfileService.Projection.FirstLevel.UnitTests.Extensions;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using TagTypeApi = Maverick.UserProfileService.Models.EnumModels.TagType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class GroupCreatedEventHandlerTests
    {
        private const int NumberOfAssignments = 10;
        private const int NumberTagAssignments = 10;
        private readonly List<ConditionObjectIdent> _conditionalMembers;
        private readonly GroupCreatedEvent _createdGroupEventWithMembersAssigned;
        private readonly GroupCreatedEvent _createdGroupEventWithMembersAssignmentAndTags;
        private readonly GroupCreatedEvent _createdGroupEventWithoutTags;
        private readonly List<FirstLevelProjectionTag> _fistLevelProjectionsTags;
        private readonly FirstLevelProjectionGroup _group;
        private readonly List<IFirstLevelProjectionProfile> _profilesToAssign;
        private readonly List<TagAssignment> _tagAssignments;

        public GroupCreatedEventHandlerTests()
        {
            _group = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();
            _conditionalMembers = MockDataGenerator.GenerateConditionObjectIdents(10, ObjectType.Group);

            _profilesToAssign = _conditionalMembers
                .Select(
                    cond => cond.Type == ObjectType.Group
                        ? MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(cond.Id) as
                            IFirstLevelProjectionProfile
                        : MockDataGenerator.GenerateFirstLevelProjectionUserWithId(cond.Id))
                .ToList();

            _profilesToAssign = _profilesToAssign.Append(_group).ToList();
            _tagAssignments = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true);
            _createdGroupEventWithoutTags = MockedSagaWorkerEventsBuilder.CreateGroupCreatedEvent(_group);
            MockedSagaWorkerEventsBuilder.CreateGroupCreatedEvent(_group, _tagAssignments);

            _createdGroupEventWithMembersAssigned =
                MockedSagaWorkerEventsBuilder.CreateGroupCreatedEvent(_group, null, _conditionalMembers);

            _createdGroupEventWithMembersAssignmentAndTags =
                MockedSagaWorkerEventsBuilder.CreateGroupCreatedEvent(_group, _tagAssignments, _conditionalMembers);

            _fistLevelProjectionsTags = _tagAssignments.Select(
                    tag => new FirstLevelProjectionTag
                    {
                        Id = tag.TagId,
                        Type = TagTypeApi.Custom,
                        Name = string.Empty
                    })
                .ToList();
        }

        private bool CheckAndRemoveRangeMember(List<RangeCondition> allIds, IList<RangeCondition> givenIds)
        {
            lock (allIds)
            {
                List<RangeCondition> listToDelete = allIds.Except(givenIds, new RangeConditionComparer()).ToList();

                if (!listToDelete.Any())
                {
                    return false;
                }

                listToDelete.ForEach(all => allIds.Remove(all));

                return true;
            }
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_event()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<GroupCreatedFirstLevelEventHandler>(services);

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

            var sut = ActivatorUtilities.CreateInstance<GroupCreatedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _createdGroupEventWithoutTags,
                    _createdGroupEventWithoutTags.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_work_with_member_and_tags()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    repo => repo.SetUpdatedAtAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<IList<ObjectIdent>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.AddTagToProfileAsync(
                        It.IsAny<FirstLevelProjectionTagAssignment>(),
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    opt => opt.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction trans, CancellationToken cancellationToken) =>
                        _fistLevelProjectionsTags.First(t => t.Id == id));

            repositoryMock.Setup(
                    repo => repo.CreateProfileAsync(
                        It.IsAny<IFirstLevelProjectionProfile>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction databaseTransaction, CancellationToken token)
                        => _profilesToAssign.Append(_group).First(profile => profile.Id == id));

            repositoryMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                          .ReturnsAsync(new List<FirstLevelRelationProfile>());

            repositoryMock.Setup(
                    repo => repo.CreateProfileAssignmentAsync(
                        It.IsAny<string>(),
                        It.IsAny<ContainerType>(),
                        It.IsAny<string>(),
                        It.IsAny<IList<RangeCondition>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var mapper = services.GetRequiredService<IMapper>();
            // ACT
            var sut = ActivatorUtilities.CreateInstance<GroupCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdGroupEventWithMembersAssignmentAndTags,
                _createdGroupEventWithMembersAssignmentAndTags.GenerateEventHeader(10),
                CancellationToken.None);

            // profile created?
            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    ItShould.BeEquivalentTo(
                        _group,
                        opt => opt.Excluding(gr => gr.SynchronizedAt).Excluding(gr => gr.IsMarkedForDeletion)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            // get all tags ?
            List<string> tagAssignments = _tagAssignments.Select(t => t.TagId).ToList();

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.Is((string id) => tagAssignments.CheckAndRemoveItem(id)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(tagAssignments);

            // assign all members?
            List<string> conditionalMemberIds = _conditionalMembers.Select(cond => cond.Id).ToList();

            repositoryMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.Is((string parentId) => parentId == _group.Id),
                    It.Is((ContainerType type) => type == ContainerType.Group),
                    It.Is((string id) => conditionalMemberIds.CheckAndRemoveItem(id)),
                    It.IsAny<IList<RangeCondition>>(),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberOfAssignments));

            Assert.Empty(conditionalMemberIds);

            // get all children from the assignees?
            List<ObjectIdent> objectIdentsAssignments =
                _conditionalMembers.Select(cond => new ObjectIdent(cond.Id, cond.Type)).ToList();

            repositoryMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.Is((ObjectIdent objectIdent) => objectIdentsAssignments.CheckAndRemoveObjectIdent(objectIdent)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(objectIdentsAssignments);

            // added Tag to the profile?
            List<string> listOfFirstLevelTagIds = _fistLevelProjectionsTags.Select(tag => tag.Id).ToList();

            repositoryMock.Verify(
                repo => repo.AddTagToProfileAsync(
                    It.Is(
                        (FirstLevelProjectionTagAssignment tag) =>
                            listOfFirstLevelTagIds.CheckAndRemoveItem(tag.TagId)),
                    It.Is((string id) => id == _group.Id),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(listOfFirstLevelTagIds);

            List<ObjectIdent> listOfUpdatedObjectIdent =
                _profilesToAssign.Select(profile => profile.ToObjectIdent())
                    .Where(objIdent => objIdent.Id != _group.Id)
                    .ToList();

            repositoryMock.Verify(
                repo => repo.SetUpdatedAtAsync(
                    It.IsAny<DateTime>(),
                    It.Is(
                        (List<ObjectIdent> objIdent) =>
                            listOfUpdatedObjectIdent.CheckAndRemoveObjectIdentList(objIdent)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(10));

            Assert.Empty(listOfUpdatedObjectIdent);
        }

        [Fact]
        public async Task Handler_should_work_with_member_without_tags()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    repo => repo.CreateProfileAsync(
                        It.IsAny<IFirstLevelProjectionProfile>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.SetUpdatedAtAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<IList<ObjectIdent>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction databaseTransaction, CancellationToken token)
                        => _profilesToAssign.First(profile => profile.Id == id));

            repositoryMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                          .ReturnsAsync(new List<FirstLevelRelationProfile>());

            repositoryMock.Setup(
                    repo => repo.CreateProfileAssignmentAsync(
                        It.IsAny<string>(),
                        It.IsAny<ContainerType>(),
                        It.IsAny<string>(),
                        It.IsAny<IList<RangeCondition>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<GroupCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdGroupEventWithMembersAssigned,
                _createdGroupEventWithMembersAssigned.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(_group, new FirstLevelGroupComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            List<string> conditionalMemberIds = _conditionalMembers.Select(cond => cond.Id).ToList();

            List<RangeCondition> allRangeConditions =
                _conditionalMembers.SelectMany(conditionalMember => conditionalMember.Conditions).ToList();

            repositoryMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.Is((string parentId) => parentId == _group.Id),
                    It.Is((ContainerType type) => type == ContainerType.Group),
                    It.Is((string id) => conditionalMemberIds.CheckAndRemoveItem(id)),
                    It.IsAny<IList<RangeCondition>>(),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberOfAssignments));

            //Assert.Empty(allRangeConditions);
            Assert.Empty(conditionalMemberIds);

            List<string> profilesIdsGetProfileWasCalled = _profilesToAssign.Select(profile => profile.Id).ToList();

            repositoryMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is((string profileId) => profilesIdsGetProfileWasCalled.CheckAndRemoveItem(profileId)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                // Get the profile of the group to which profiles should be assigned
                Times.Exactly(NumberOfAssignments + 1));

            Assert.Empty(profilesIdsGetProfileWasCalled);

            List<ObjectIdent> objectIdentsAssignments =
                _conditionalMembers.Select(cond => new ObjectIdent(cond.Id, cond.Type)).ToList();

            repositoryMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.Is((ObjectIdent objectIdent) => objectIdentsAssignments.CheckAndRemoveObjectIdent(objectIdent)),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberTagAssignments));

            Assert.Empty(objectIdentsAssignments);
        }

        [Fact]
        public async Task Handler_should_work_without_tags_and_members()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);

            repositoryMock.Setup(
                    repo => repo.GetTagAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(new FirstLevelProjectionTag());

            var sagaService = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            repositoryMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.CreateProfileAsync(
                        It.IsAny<IFirstLevelProjectionProfile>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .Returns(Task.CompletedTask);

            var sut = ActivatorUtilities.CreateInstance<GroupCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdGroupEventWithoutTags,
                _createdGroupEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repositoryMock.Verify(
                repo => repo.CreateProfileAsync(
                    It.Is(_group, new FirstLevelGroupComparer()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            repositoryMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repositoryMock.Verify(
                repo => repo.CreateProfileAssignmentAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContainerType>(),
                    It.IsAny<string>(),
                    It.IsAny<RangeCondition[]>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
