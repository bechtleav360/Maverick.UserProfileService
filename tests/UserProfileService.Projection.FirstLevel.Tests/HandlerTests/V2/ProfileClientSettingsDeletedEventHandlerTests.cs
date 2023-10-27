using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class ProfileClientSettingsDeletedEventHandlerTests
    {
        private readonly FirstLevelProjectionGroup _groupWithUser;
        private readonly IFirstLevelProjectionProfile _lonelyUser;
        private readonly FirstLevelProjectionUser _userOfGroup;

        public ProfileClientSettingsDeletedEventHandlerTests()
        {
            _lonelyUser = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _groupWithUser = MockDataGenerator.GenerateFirstLevelProjectionGroupWithId("group-1");
            _userOfGroup = MockDataGenerator.GenerateFirstLevelProjectionUserWithId("user-1");
        }

        private Mock<IFirstLevelProjectionRepository> CreateFirstLevelMockForGroupAndUserTestCase(
            IDatabaseTransaction transaction,
            CancellationToken cancellationTokenToBeUsed)
        {
            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            var callingSequence = new MockSequence();

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.UnsetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        cancellationTokenToBeUsed))
                .Returns(Task.CompletedTask);

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.Is<string>(pId => pId == _groupWithUser.Id),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = _groupWithUser.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToKeep,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _groupWithUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            }
                        }
                    });

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelRelationProfile>
                    {
                        new FirstLevelRelationProfile
                        {
                            Profile = _userOfGroup,
                            Relation = FirstLevelMemberRelation.DirectMember
                        }
                    });

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.Is<string>(pId => pId == _userOfGroup.Id),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = _userOfGroup.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToKeep,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _groupWithUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            }
                        }
                    });

            return repoMock;
        }

        private Mock<IFirstLevelProjectionRepository> CreateFirstLevelMockForUserInsideGroupTestCase(
            IDatabaseTransaction transaction,
            CancellationToken cancellationTokenToBeUsed)
        {
            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            var callingSequence = new MockSequence();

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.UnsetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        cancellationTokenToBeUsed))
                .Returns(Task.CompletedTask);

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.Is<string>(pId => pId == _groupWithUser.Id),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = _groupWithUser.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToKeep,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _groupWithUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            }
                        }
                    });

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelRelationProfile>
                    {
                        new FirstLevelRelationProfile
                        {
                            Profile = _userOfGroup,
                            Relation = FirstLevelMemberRelation.DirectMember
                        }
                        
                    });

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.Is<string>(pId => pId == _userOfGroup.Id),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = _userOfGroup.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToKeep,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _groupWithUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            }
                        }
                    });

            return repoMock;
        }

        private Mock<IFirstLevelProjectionRepository> CreateFirstLevelMockForUserInsideGroupWithSameKeyTestCase(
            IDatabaseTransaction transaction,
            CancellationToken cancellationTokenToBeUsed)
        {
            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            var callingSequence = new MockSequence();

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.UnsetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        cancellationTokenToBeUsed))
                .Returns(Task.CompletedTask);

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.Is<string>(pId => pId == _userOfGroup.Id),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = _userOfGroup.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingDoubleKey,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _groupWithUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            }
                        }
                    });

            repoMock.InSequence(callingSequence)
                .Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FirstLevelRelationProfile>());

            return repoMock;
        }

        [Fact]
        public async Task Delete_client_settings_key_of_lonely_user_should_work()
        {
            // arrange
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();
            
            

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        ItShould.BeEquivalentTo(
                            transaction,
                            opt => opt.RespectingRuntimeTypes()),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToKeep,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsValueToKeep,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    _lonelyUser.Id, new List<RangeCondition>
                                    {
                                        new RangeCondition()
                                    }
                                }
                            },
                            ProfileId = _lonelyUser.Id
                        }
                    });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_lonelyUser);

            var sagaMock = new MockSagaService();
            var ct = new CancellationToken();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaMock);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsDeletedFirstLevelEventHandler>(services);

            ProfileClientSettingsDeletedEvent lonelyUserEvent = MockedSagaWorkerEventsBuilder
                .CreateProfileClientSettingsDeletedEvent(
                    _lonelyUser,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete);

            // act
            await sut.HandleEventAsync(
                lonelyUserEvent,
                lonelyUserEvent.GenerateEventHeader(255),
                ct);

            // assert
            repoMock.Verify(
                repo => repo.UnsetClientSettingsAsync(
                    _lonelyUser.Id,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete,
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    ct),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _lonelyUser.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            IReadOnlyDictionary<Guid, List<EventTuple>> tuple = sagaMock.GetDictionary();

            AssertionHelper.EventTupleEquivalent(
                tuple.First().Value,
                InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsOfLonelyUserEventTuple(
                    _lonelyUser.Id,
                    services.GetRequiredService<IStreamNameResolver>()));
        }

        /// <summary>
        ///     Test case:<br />
        ///     - group as root with one member (user) <br />
        ///     - one of two keys of the group will be deleted <br />
        ///     - the user will get a notification (aka event) that the inherited client settings have been changed <br />
        ///     - the user profile does not have an own set of client settings <br /><br />
        ///     We expect the following generated events for the second level projection: <br />
        ///     The group (or exactly the stream of the group) <br />
        ///     1. Unset client settings <br />
        ///     2. Calculated client settings <br />
        ///     3. invalidate client settings (including only the one key that should be kept) <br /><br />
        ///     The user of the group <br />
        ///     1. Calculated client settings <br />
        ///     2. invalidate (including only the one key that should be kept)
        /// </summary>
        [Fact]
        public async Task Delete_client_settings_key_of_group_with_user_should_work()
        {
            // arrange
            var ct = new CancellationToken();
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                CreateFirstLevelMockForGroupAndUserTestCase(transaction, ct);

            var sagaMock = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaMock);
                });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_groupWithUser);
            
            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsDeletedFirstLevelEventHandler>(services);

            ProfileClientSettingsDeletedEvent groupWithUserEvent = MockedSagaWorkerEventsBuilder
                .CreateProfileClientSettingsDeletedEvent(
                    _groupWithUser,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete);

            // act
            await sut.HandleEventAsync(
                groupWithUserEvent,
                groupWithUserEvent.GenerateEventHeader(255),
                ct);

            // assert
            repoMock.Verify(
                repo => repo.UnsetClientSettingsAsync(
                    _groupWithUser.Id,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete,
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    ct),
                Times.Once);

            IReadOnlyDictionary<Guid, List<EventTuple>> tuple = sagaMock.GetDictionary();

            AssertionHelper.EventTupleEquivalent(
                tuple.First().Value,
                InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsOfRootGroupEventTuple(
                    _groupWithUser.Id,
                    _userOfGroup.Id,
                    services.GetRequiredService<IStreamNameResolver>()));
        }

        [Fact]
        public async Task Delete_client_settings_key_of_user_inside_group_should_work()
        {
            // arrange
            var ct = new CancellationToken();
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                CreateFirstLevelMockForUserInsideGroupTestCase(transaction, ct);

            var sagaMock = new MockSagaService();

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_groupWithUser);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaMock);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsDeletedFirstLevelEventHandler>(services);

            ProfileClientSettingsDeletedEvent groupWithUserEvent = MockedSagaWorkerEventsBuilder
                .CreateProfileClientSettingsDeletedEvent(
                    _groupWithUser,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete);

            // act
            await sut.HandleEventAsync(
                groupWithUserEvent,
                groupWithUserEvent.GenerateEventHeader(255),
                ct);

            // assert
            repoMock.Verify(
                repo => repo.UnsetClientSettingsAsync(
                    _groupWithUser.Id,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingsKeyToDelete,
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    ct),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _groupWithUser.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            
            IReadOnlyDictionary<Guid, List<EventTuple>> tuple = sagaMock.GetDictionary();

            AssertionHelper.EventTupleEquivalent(
                tuple.First().Value,
                InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsOfRootGroupEventTuple(
                    _groupWithUser.Id,
                    _userOfGroup.Id,
                    services.GetRequiredService<IStreamNameResolver>()));
        }

        [Fact]
        public async Task Delete_client_settings_key_of_user_inside_group_with_same_key_should_work()
        {
            // arrange
            var ct = new CancellationToken();
            IDatabaseTransaction transaction = MockProvider.GetDefaultTransactionMock();

            Mock<IFirstLevelProjectionRepository> repoMock =
                CreateFirstLevelMockForUserInsideGroupWithSameKeyTestCase(transaction, ct);

            var sagaMock = new MockSagaService();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaMock);
                });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_userOfGroup);

            
            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsDeletedFirstLevelEventHandler>(services);

            ProfileClientSettingsDeletedEvent groupWithUserEvent = MockedSagaWorkerEventsBuilder
                .CreateProfileClientSettingsDeletedEvent(
                    _userOfGroup,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingDoubleKey);

            // act
            await sut.HandleEventAsync(
                groupWithUserEvent,
                groupWithUserEvent.GenerateEventHeader(255),
                ct);

            // assert
            repoMock.Verify(
                repo => repo.UnsetClientSettingsAsync(
                    _userOfGroup.Id,
                    InputSagaWorkerEventsOutputEventTuple.ClientSettingDoubleKey,
                    ItShould.BeEquivalentTo(
                        transaction,
                        opt => opt.RespectingRuntimeTypes()),
                    ct),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetProfileAsync(
                    It.Is<string>(id => id == _userOfGroup.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            IReadOnlyDictionary<Guid, List<EventTuple>> tuple = sagaMock.GetDictionary();

            AssertionHelper.EventTupleEquivalent(
                tuple.First().Value,
                InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsOfUserInsideGroupWithSameKeyEventTuple(
                    _userOfGroup.Id,
                    services.GetRequiredService<IStreamNameResolver>()));
        }
    }
}
