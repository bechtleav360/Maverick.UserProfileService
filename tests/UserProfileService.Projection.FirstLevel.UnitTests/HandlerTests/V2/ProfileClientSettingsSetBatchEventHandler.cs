using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
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
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class ProfileClientSettingsSetBatchEventHandlerTest
    {
        private readonly KeyValuePair<string, string> _clientSettings;
        private readonly ProfileClientSettingsSetBatchEvent _clientSettingsEventWithChilds;
        private readonly ProfileClientSettingsSetBatchEvent _clientSettingsOnyWithUser;
        private readonly FirstLevelProjectionGroup _groupChild;
        private readonly ProfileIdent _groupIdent;
        private readonly FirstLevelProjectionGroup _projectionGroup;
        private readonly int _numberOfMembers = 10;
        private readonly DateTime _start;
        private readonly FirstLevelProjectionUser _userChild;
        private readonly List<ProfileIdent> _userProfileIdents;

        private readonly Dictionary<string, IFirstLevelProjectionProfile> _returnedProfiles =
            new Dictionary<string, IFirstLevelProjectionProfile>();

        public ProfileClientSettingsSetBatchEventHandlerTest()
        {
            _start = DateTime.UtcNow;

            _clientSettings = new KeyValuePair<string, string>(
                "Outlook",
                "{\"Value\":\"O365Premium\"}");

            _userProfileIdents = MockDataGenerator.GenerateProfileIdent(10, ProfileKind.User);
            
            

            _clientSettingsOnyWithUser =
                MockedSagaWorkerEventsBuilder.CreateProfileClientSettingsSetBatchEvent(
                    _userProfileIdents,
                    _clientSettings,
                    _start);

            _userChild = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _groupChild = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();
            _groupIdent = MockDataGenerator.GenerateProfileIdent(1, ProfileKind.Group).Single();
            _projectionGroup = MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_groupIdent.Id);

            _clientSettingsEventWithChilds = MockedSagaWorkerEventsBuilder.CreateProfileClientSettingsSetBatchEvent(
                _userProfileIdents.Append(_groupIdent).ToList(),
                _clientSettings,
                _start);

            foreach (ProfileIdent profile in _userProfileIdents)
            {
                FirstLevelProjectionUser tmp = MockDataGenerator.GenerateFirstLevelProjectionUserWithId(profile.Id);
                _returnedProfiles.TryAdd(tmp.Id, tmp);
            }

            _returnedProfiles.TryAdd(_projectionGroup.Id, _projectionGroup);
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>(MockBehavior.Strict);

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        private string ExtractNewLinesFromString(string jObjectString)
        {
            string result = jObjectString.Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace(" ", string.Empty);

            return result;
        }

        private bool CheckAndRemoveItem(List<string> ids, string id)
        {
            lock (ids)
            {
                int position = ids.IndexOf(id);

                if (position == -1)
                {
                    return false;
                }

                ids.RemoveAt(position);

                return true;
            }
        }

        private bool CheckAndRemoveObjectIdent(List<ObjectIdent> ids, ObjectIdent id)
        {
            lock (ids)
            {
                ObjectIdent toRemove = ids.FirstOrDefault(objId => objId.Id == id.Id && objId.Type == id.Type);

                if (toRemove == null)
                {
                    return false;
                }

                ids.Remove(toRemove);

                return true;
            }
        }

        [Fact]
        public async Task handler_should_work_with_user_as_profile_setting()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repoMock.Setup(
                    repo => repo.SetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<FirstLevelRelationProfile>());

            repoMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    "Group-3039FD56-F36E-4217-AC21-0AD196681FF6", new List<RangeCondition>
                                    {
                                        new RangeCondition(_start, _start.AddHours(23)),
                                        new RangeCondition(_start, _start.AddYears(23))
                                    }
                                }
                            },
                            ProfileId = "CBDD438A-FC1E-4BFB-943D-92618866C760",
                            SettingsKey = "Outlook",
                            UpdatedAt = _start,
                            Weight = 1.0,
                            Hops = 1,
                            Value = "0365-TestVersion"
                        },

                        new FirstLevelProjectionsClientSetting
                        {
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            ProfileId = "CBDD438A-FC1E-4BFB-943D-92618866C760",
                            SettingsKey = "Outlook",
                            UpdatedAt = _start,
                            Hops = 0,
                            Weight = 1.0,
                            Value = "0365-Premium"
                        }
                    });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        (string id, IDatabaseTransaction _, CancellationToken __)
                            => _returnedProfiles[id]);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var streamNameResolver = services.GetService<IStreamNameResolver>();

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetBatchFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _clientSettingsOnyWithUser,
                _clientSettingsOnyWithUser.GenerateEventHeader(
                    10,
                    streamNameResolver?.GetStreamName(
                        new
                            ObjectIdent(Guid.NewGuid().ToString(), ObjectType.User))));

            List<string> userIds = _userProfileIdents.Select(x => x.Id).ToList();

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string profileId) => CheckAndRemoveItem(userIds, profileId)),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(_clientSettings.Value)),
                    It.Is((string clientSettingsKey) => clientSettingsKey == _clientSettings.Key),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(10));

            repoMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.IsAny<ObjectIdent>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            Assert.Empty(userIds);

            List<string> userRecalculateClientSettings = _userProfileIdents.Select(x => x.Id).ToList();

            repoMock.Verify(
                repo => repo.GetCalculatedClientSettingsAsync(
                    It.Is((string profileId) => CheckAndRemoveItem(userRecalculateClientSettings, profileId)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(_numberOfMembers));

            Assert.Empty(userRecalculateClientSettings);
        }

        [Fact]
        public async Task handler_should_work_with_group_as_profile_and_children()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repoMock.Setup(
                    repo => repo.SetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent ident, IDatabaseTransaction transaction, CancellationToken token) =>
                    {
                        if (ident.Id == _groupIdent.Id && _groupIdent.ProfileKind == ProfileKind.Group)
                        {
                            return new List<FirstLevelRelationProfile>
                                   {
                                       new FirstLevelRelationProfile
                                       {
                                           Profile = _groupChild,
                                           Relation = FirstLevelMemberRelation.DirectMember
                                       },
                                       new FirstLevelRelationProfile
                                       {
                                           Profile = _userChild,
                                           Relation = FirstLevelMemberRelation.DirectMember
                                       }
                                   };
                        }

                        return new List<FirstLevelRelationProfile>();
                    });

            repoMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    "Group-3039FD56-F36E-4217-AC21-0AD196681FF6", new List<RangeCondition>
                                    {
                                        new RangeCondition(_start, _start.AddHours(23)),
                                        new RangeCondition(_start, _start.AddYears(23))
                                    }
                                }
                            },
                            ProfileId = _projectionGroup.Id,
                            SettingsKey = "Outlook",
                            UpdatedAt = _start,
                            Weight = 1.0,
                            Hops = 1,
                            Value = "0365-TestVersion"
                        }
                    });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        (string id, IDatabaseTransaction _, CancellationToken __)
                            => _returnedProfiles[id]);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetBatchFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _clientSettingsEventWithChilds,
                _clientSettingsEventWithChilds.GenerateEventHeader(12),
                CancellationToken.None);

            List<string> recalculateIds = _userProfileIdents
                .Concat(
                    new List<ProfileIdent>
                    {
                        _groupIdent
                    })
                .Select(ob => ob.Id)
                .ToList();

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string id) => CheckAndRemoveItem(recalculateIds, id)),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(_clientSettings.Value)),
                    It.Is((string clientSettingsKey) => clientSettingsKey == _clientSettings.Key),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(_numberOfMembers + 1));

            Assert.Empty(recalculateIds);

            repoMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.Is(
                        (ObjectIdent objectProfile) =>
                            objectProfile.Id == _groupIdent.Id && objectProfile.Type == ObjectType.Group),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once());

            List<string> childIdsRecalculate = _userProfileIdents
                .Concat(
                    new List<ProfileIdent>
                    {
                        _groupIdent,
                        new ProfileIdent(_userChild.Id, ProfileKind.User),
                        new ProfileIdent(_groupChild.Id, ProfileKind.Group)
                    })
                .Select(ob => ob.Id)
                .ToList();

            repoMock.Verify(
                repo => repo.GetCalculatedClientSettingsAsync(
                    It.Is((string profileId) => CheckAndRemoveItem(childIdsRecalculate, profileId)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(13));
            
            Assert.Empty(childIdsRecalculate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task handler_should_work_with_input_output_event(int hops)
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            repoMock.Setup(
                    repo => repo.SetClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    repo => repo.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<FirstLevelRelationProfile>());

            repoMock.Setup(
                    repo => repo.SaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        new FirstLevelProjectionUser
                        {
                            Id = InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchUser.Id
                        });

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<FirstLevelProjectionsClientSetting>
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    "Group-3039FD56-F36E-4217-AC21-0AD196681FF6", new List<RangeCondition>
                                    {
                                        new RangeCondition(_start, _start.AddHours(23)),
                                        new RangeCondition(_start, _start.AddYears(23))
                                    }
                                }
                            },
                            ProfileId = InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchUser.Id,
                            SettingsKey = InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchResolved.Key,
                            UpdatedAt = _start,
                            Weight = 1.0,
                            Hops = hops,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchResolved
                                .CalculatedSettings
                        }
                    });

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetBatchFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                InputSagaWorkerEventsOutputEventTuple.SetClientSettingsBatchEvent,
                InputSagaWorkerEventsOutputEventTuple.SetClientSettingsBatchEvent.GenerateEventHeader(12),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string id) => id == InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchUser.Id),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(
                                InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchResolved.CalculatedSettings)),
                    It.Is(
                        (string clientSettingsKey) => clientSettingsKey
                            == InputSagaWorkerEventsOutputEventTuple
                                .ClientSettingsBatchResolved
                                .Key),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.IsAny<ObjectIdent>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            repoMock.Verify(
                repo => repo.GetCalculatedClientSettingsAsync(
                    It.Is(
                        (string profileId) =>
                            profileId == InputSagaWorkerEventsOutputEventTuple.ClientSettingsBatchUser.Id),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            List<EventTuple> result = sagaService.GetDictionary().Values.First();

            ((ProfileClientSettingsSet)result[0].Event).ClientSettings =
                ExtractNewLinesFromString(((ProfileClientSettingsSet)result[0].Event).ClientSettings);

            List<EventTuple> resolvedClientSettingsUserBatchEventTuple = hops > 0
                ? InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsInheritedUserBatchEventTuple
                : InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsUserBatchEventTuple;

            resolvedClientSettingsUserBatchEventTuple.Should()
                                                     .BeEquivalentTo(
                                                         result,
                                                         opt => opt.RespectingRuntimeTypes()
                                                                   .Excluding(evt => evt.Event.EventId)
                                                                   .Excluding(p => p.Event.MetaData.Timestamp)
                                                                   .Excluding(p => p.Event.MetaData.Batch)
                                                                   .WithStrictOrdering());
            }
        }
    }
