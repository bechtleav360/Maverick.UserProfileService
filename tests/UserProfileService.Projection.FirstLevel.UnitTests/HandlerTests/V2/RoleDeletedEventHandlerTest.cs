using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Comparer;
using UserProfileService.Projection.FirstLevel.UnitTests.Extensions;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using static UserProfileService.Projection.FirstLevel.UnitTests.InputSagaWorkerEventsOutputEventTuple;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class RoleDeletedEventHandlerTest
    {
        private const int NumberOfAffectedFunctionsByRoleDeleted = 5;
        private const int NumbersOfChildrenForDirectMember = 10;
        private const int NumbersOfDirectMembers = 10;
        private readonly Dictionary<ObjectIdent, IList<IFirstLevelProjectionProfile>> _childrenOfMember;
        private readonly Dictionary<ObjectIdent, List<ObjectIdent>> _directMembersForFunctionAndRole;
        private readonly List<ObjectIdent> _functionAndRoleAsObjectIdent;
        private readonly IList<FirstLevelProjectionFunction> _functions;
        private readonly List<ObjectIdentPath> _resultForGetGetAllRelevantEntities;
        private readonly FirstLevelProjectionRole _role;
        private readonly ObjectIdent _roleAsObjectIdent;
        private readonly RoleDeletedEvent _roleDeletedEvents;

        public RoleDeletedEventHandlerTest()
        {
            _role = MockDataGenerator.GenerateFirstLevelRole();

            _functions = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances(
                NumberOfAffectedFunctionsByRoleDeleted,
                _role);

            _roleAsObjectIdent = new ObjectIdent(_role.Id, ObjectType.Role);

            _functionAndRoleAsObjectIdent = _functions.Select(func => new ObjectIdent(func.Id, ObjectType.Function))
                .Append(_roleAsObjectIdent)
                .ToList();

            _directMembersForFunctionAndRole = FillDirectMembers(_functionAndRoleAsObjectIdent);

            List<ObjectIdent> listOfDirectMembers =
                _directMembersForFunctionAndRole.SelectMany(direMem => direMem.Value).ToList();

            _childrenOfMember = FillMembersWithChildren(listOfDirectMembers);
            _roleDeletedEvents = MockedSagaWorkerEventsBuilder.CreateRoleDeletedEvent(_role);

            // childrenMember are only added to this list, to see if we oly get the functions.
            // childrenMembers can only be groups or users

            List<ObjectIdent> roleAsObjectIdent = _directMembersForFunctionAndRole[_roleAsObjectIdent].ToList();

            _resultForGetGetAllRelevantEntities =
                _functions.Select(func => new ObjectIdentPath(func.Id, ObjectType.Function))
                    .Concat(roleAsObjectIdent.Select(x => new ObjectIdentPath(x.Id, x.Type)))
                    .ToList();
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        private static Dictionary<ObjectIdent, IList<IFirstLevelProjectionProfile>> FillMembersWithChildren(
            IList<ObjectIdent> children)
        {
            var childrenOfMember =
                new Dictionary<ObjectIdent, IList<IFirstLevelProjectionProfile>>(new ObjectIdentComparer());

            foreach (ObjectIdent directMember in children)
            {
                if (directMember.Type == ObjectType.User)
                {
                    childrenOfMember.Add(directMember, new List<IFirstLevelProjectionProfile>());
                }
                else
                {
                    childrenOfMember.Add(
                        directMember,
                        MockDataGenerator.GenerateFirstLevelProfiles(
                            NumbersOfChildrenForDirectMember,
                            directMember.Type));
                }
            }

            return childrenOfMember;
        }

        private static Dictionary<ObjectIdent, List<ObjectIdent>> FillDirectMembers(
            List<ObjectIdent> entitiesTheMembersBelongsTo)
        {
            var
                directMembers = new Dictionary<ObjectIdent, List<ObjectIdent>>(new ObjectIdentComparer());

            foreach (ObjectIdent entity in entitiesTheMembersBelongsTo)
            {
                directMembers.Add(
                    entity,
                    MockDataGenerator.GenerateObjectMemberForEntity(
                        NumbersOfDirectMembers,
                        entity.Type));
            }

            return directMembers;
        }

        [Fact]
        public async Task Handler_should_work_with_members_and_children()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    repo => repo.GetContainerMembersAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (
                        ObjectIdent parent,
                        IDatabaseTransaction tran,
                        CancellationToken token) =>
                    {
                        bool found = _directMembersForFunctionAndRole.TryGetValue(
                            parent,
                            out List<ObjectIdent> result);

                        if (!found)
                        {
                            throw new InstanceNotFoundException(
                                $"The instance with the id = {parent.Id} and Type {parent.Type} not found.");
                        }

                        return result;
                    });

            repositoryMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent parent, IDatabaseTransaction tran, CancellationToken token) =>
                    {
                        bool found = _childrenOfMember.TryGetValue(
                            parent,
                            out IList<IFirstLevelProjectionProfile> result);

                        if (!found)
                        {
                            throw new InstanceNotFoundException(
                                $"The instance with the id = {parent.Id} and Type {parent.Type} not found.");
                        }

                        return result.Select(
                                         profile => new FirstLevelRelationProfile(
                                             profile,
                                             FirstLevelMemberRelation.DirectMember))
                                     .ToList();
                    });

            repositoryMock.Setup(
                    repo => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _resultForGetGetAllRelevantEntities);

            repositoryMock.Setup(
                    repo => repo.DeleteRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.DeleteFunctionAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.GetRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _role);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            //Act
            var roleDeletedHandler = ActivatorUtilities.CreateInstance<RoleDeletedFirstLevelEventHandler>(services);

            await roleDeletedHandler.HandleEventAsync(_roleDeletedEvents, _roleDeletedEvents.GenerateEventHeader(10));

            // Assert
            repositoryMock.Verify(
                repo => repo.GetRoleAsync(
                    It.Is<string>(id => id == _role.Id),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            repositoryMock.Verify(
                repo => repo.DeleteRoleAsync(
                    It.Is<string>(id => id == _role.Id),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            List<string> functionsToDeleteIds = _functions.Select(func => func.Id).ToList();

            repositoryMock.Verify(
                repo => repo.DeleteFunctionAsync(
                    It.Is<string>(id => functionsToDeleteIds.CheckAndRemoveItem(id)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(NumberOfAffectedFunctionsByRoleDeleted));

            List<ObjectIdent> entitiesToDelete = _functions
                .Select(func => new ObjectIdent(func.Id, ObjectType.Function))
                .Append(_roleAsObjectIdent)
                .ToList();

            repositoryMock.Verify(
                repo => repo.GetContainerMembersAsync(
                    It.Is<ObjectIdent>(container => entitiesToDelete.CheckAndRemoveObjectIdent(container)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                // plus the role Itself
                Times.Exactly(NumberOfAffectedFunctionsByRoleDeleted + 1));

            repositoryMock.Verify(
                repo => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    It.Is<ObjectIdent>(container => container.Id == _role.Id && container.Type == ObjectType.Role),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Once);

            List<ObjectIdent> membersCalledForChildren = _childrenOfMember.Select(member => member.Key)
                .Where(member => member.Type != ObjectType.User)
                .ToList();

            int countOfChildCalls = membersCalledForChildren.Count;

            repositoryMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.Is<ObjectIdent>(container => membersCalledForChildren.CheckAndRemoveObjectIdent(container)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    CancellationToken.None),
                Times.Exactly(countOfChildCalls));
        }

        [Fact]
        public async Task Handler_should_work_input_output_only_role_deleted_test()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repositoryMock.Setup(
                    repo => repo.GetContainerMembersAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<ObjectIdent>());

            repositoryMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                          .ReturnsAsync(() => new List<FirstLevelRelationProfile>());

            repositoryMock.Setup(
                    repo => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<ObjectIdentPath>());

            repositoryMock.Setup(
                    repo => repo.DeleteRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.DeleteFunctionAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.GetRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => RoleToDelete);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            //Act
            var roleDeletedHandler = ActivatorUtilities.CreateInstance<RoleDeletedFirstLevelEventHandler>(services);

            await roleDeletedHandler.HandleEventAsync(
                InputSagaWorkerEventsOutputEventTuple.RoleDeletedEvent,
                InputSagaWorkerEventsOutputEventTuple.RoleDeletedEvent.GenerateEventHeader(10));

            List<EventTuple> eventTupleCreated = sagaService.GetDictionary().Values.First();

            eventTupleCreated.Should()
                .BeEquivalentTo(
                    ResolvedOnlyRoleShouldBeDelete,
                    opt => opt.Excluding(p => p.Event.EventId)
                        .Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .RespectingRuntimeTypes());
        }

        // Is state: RootRole
        //              |
        //           RoleGroup
        //            /     \
        //  RoleFirstUser   RoleSecondUser
        //
        // RootRole is in two functions: FirstRoleFunction and SecondRoleFunction
        //
        //      FirstRoleFunction
        //              |
        //     FirstUserRoleFunction
        //
        //      SecondRoleFunction
        //              |             
        //    SecondRoleFunctionGroup   
        //         /       \ 
        //     SubGroup    SecondRoleFunctionUser
        //      /     \
        // SubUserOne  SubUserTwo
        // 
        // We are deleting the RootRole in this case
        [Fact]
        public async Task Handler_should_work_input_output_complex_one()
        {
            //arrange

            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repositoryMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            var directMembers = new Dictionary<ObjectIdent, List<ObjectIdent>>
            {
                {
                    new ObjectIdent(RootRole.Id, ObjectType.Role), new List<ObjectIdent>
                    {
                        new ObjectIdent(RoleRootGroup.Id, ObjectType.Group)
                    }
                },
                {
                    FirstRoleFunction, new List<ObjectIdent>
                    {
                        new ObjectIdent(FirstUserRoleFunction.Id, ObjectType.User)
                    }
                },
                {
                    SecondRoleFunction, new List<ObjectIdent>
                    {
                        new ObjectIdent(SecondRoleFunctionGroup.Id, ObjectType.Group)
                    }
                }
            };

            repositoryMock.Setup(
                    repo => repo.GetContainerMembersAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (
                        ObjectIdent parent,
                        IDatabaseTransaction databaseTransaction,
                        CancellationToken cancellationToken) =>
                    {
                        var containingDirectMembers = new Dictionary<ObjectIdent, List<ObjectIdent>>(
                            directMembers,
                            new ObjectIdentComparer());

                        bool found = containingDirectMembers.TryGetValue(
                            parent,
                            out List<ObjectIdent> directMembersObjectIdents);

                        if (!found)
                        {
                            throw new InstanceNotFoundException(
                                $"No member could  found for the id = {parent.Id} and type = {parent.Type}");
                        }

                        return directMembersObjectIdents;
                    });

            var childrenForTheMembers = new Dictionary<ObjectIdent, List<IFirstLevelProjectionProfile>>
            {
                {
                    new ObjectIdent(RoleRootGroup.Id, ObjectType.Group), new List<IFirstLevelProjectionProfile>
                    {
                        RoleFirstUser,
                        RoleSecondUser
                    }
                },
                {
                    new ObjectIdent(SecondRoleFunctionGroup.Id, ObjectType.Group),
                    new List<IFirstLevelProjectionProfile>
                    {
                        SubGroup,
                        SubUserOne,
                        SubUserTwo,
                        SecondRoleFunctionUser
                    }
                }
            };

            repositoryMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (
                        ObjectIdent parent,
                        IDatabaseTransaction databaseTransaction,
                        CancellationToken cancellationToken) =>
                    {
                        var containingChildrenOfMembers =
                            new Dictionary<ObjectIdent, List<IFirstLevelProjectionProfile>>(
                                childrenForTheMembers,
                                new ObjectIdentComparer());

                        bool found = containingChildrenOfMembers.TryGetValue(
                            parent,
                            out List<IFirstLevelProjectionProfile> childrenOfMembers);

                        if (!found)
                        {
                            throw new InstanceNotFoundException(
                                $"No member could  found for the id = {parent.Id} and type = {parent.Type}");
                        }

                        return childrenOfMembers.Select(
                                                    profile => new FirstLevelRelationProfile(
                                                        profile,
                                                        FirstLevelMemberRelation.DirectMember))
                                                .ToList();
                    });

            repositoryMock.Setup(
                    repo => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    () => new List<ObjectIdentPath>
                    {
                        FirstRoleFunction.ToObjectIdentPath(),
                        SecondRoleFunction.ToObjectIdentPath()
                    });

            repositoryMock.Setup(
                    repo => repo.DeleteRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.DeleteFunctionAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repositoryMock.Setup(
                    repo => repo.GetRoleAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => RootRole);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repositoryMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            //Act
            var roleDeletedHandler = ActivatorUtilities.CreateInstance<RoleDeletedFirstLevelEventHandler>(services);

            await roleDeletedHandler.HandleEventAsync(
                RoleDeletedEventComplexCase,
                RoleDeletedEventComplexCase.GenerateEventHeader(10));

            List<EventTuple> eventTupleCreated = sagaService.GetDictionary().Values.First();

            eventTupleCreated.Should()
                .BeEquivalentTo(
                    ResolvedRoleDeletedComplexCaseEventTuple,
                    opt => opt.Excluding(p => p.Event.EventId)
                        .Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .RespectingRuntimeTypes());
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_event()
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

            var sut = ActivatorUtilities.CreateInstance<RoleDeletedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    null,
                    _roleDeletedEvents.GenerateEventHeader(10),
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handler_should_fail_because_of_null_streamHeader()
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

            var sut = ActivatorUtilities.CreateInstance<RoleDeletedFirstLevelEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(
                    _roleDeletedEvents,
                    null,
                    CancellationToken.None));
        }
    }
}
