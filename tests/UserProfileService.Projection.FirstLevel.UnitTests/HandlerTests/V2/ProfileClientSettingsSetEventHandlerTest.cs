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
    public class ProfileClientSettingsSetEventHandlerTest
    {
        private readonly Dictionary<string, string> _clientSettings;
        private readonly ProfileClientSettingsSetEvent _eventObjectOnlyWithGroupAndChildren;
        private readonly ProfileClientSettingsSetEvent _eventObjectOnlyWithUser;
        private readonly FirstLevelProjectionGroup _group;
        private readonly FirstLevelProjectionGroup _groupChild;
        private readonly DateTime _start;
        private readonly FirstLevelProjectionUser _user;
        private readonly FirstLevelProjectionUser _userChild;

        public ProfileClientSettingsSetEventHandlerTest()
        {
            _clientSettings = new Dictionary<string, string>
            {
                { "Outlook", "{\"Value\":\"O365Premium\"}" }
            };

            _user = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _userChild = MockDataGenerator.GenerateFirstLevelProjectionUser().Single();
            _group = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();
            _groupChild = MockDataGenerator.GenerateFirstLevelProjectionGroup().Single();

            _eventObjectOnlyWithUser =
                MockedSagaWorkerEventsBuilder.CreateProfileClientSettingsSetEvent(_user, _clientSettings);

            _eventObjectOnlyWithGroupAndChildren =
                MockedSagaWorkerEventsBuilder.CreateProfileClientSettingsSetEvent(_group, _clientSettings);

            _start = DateTime.UtcNow;
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

        private string ExtractNewLinesFromString(string jObjectString)
        {
            string result = jObjectString.Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace(" ", string.Empty);

            return result;
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
                    new List<FirstLevelRelationProfile>
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
                            ProfileId = "CBDD438A-FC1E-4BFB-943D-92618866C760",
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
                        new FirstLevelProjectionGroup
                        {
                            Id = _group.Id
                        });
            
            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _eventObjectOnlyWithGroupAndChildren,
                _eventObjectOnlyWithGroupAndChildren.GenerateEventHeader(12),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string id) => id == _group.Id),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(_clientSettings.Values.First())),
                    It.Is((string clientSettingsKey) => clientSettingsKey == _clientSettings.Keys.First()),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetAllChildrenAsync(
                    It.Is(
                        (ObjectIdent objectProfile) =>
                            objectProfile.Id == _group.Id && objectProfile.Type == ObjectType.Group),
                    It.Is<IDatabaseTransaction>(
                        t =>
                            ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            var recalculateChildrenAndOwn = new List<string>
            {
                _group.Id,
                _userChild.Id,
                _groupChild.Id
            };

            repoMock.Verify(
                repo => repo.GetCalculatedClientSettingsAsync(
                    It.Is((string profileId) => CheckAndRemoveItem(recalculateChildrenAndOwn, profileId)),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(3));

            Assert.Empty(recalculateChildrenAndOwn);
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
                            ProfileId = InputSagaWorkerEventsOutputEventTuple.ClientSettingsUser.Id,
                            SettingsKey =
                                InputSagaWorkerEventsOutputEventTuple.ClientSettingsCalculatedResolved.Key,
                            UpdatedAt = _start,
                            Weight = 1.0,
                            Hops = hops,
                            Value = InputSagaWorkerEventsOutputEventTuple.ClientSettingsCalculatedResolved
                                .CalculatedSettings
                        }
                    });

            repoMock.Setup(
                        repo => repo.GetProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<IDatabaseTransaction>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        new FirstLevelProjectionUser
                        {
                            Id = InputSagaWorkerEventsOutputEventTuple.ClientSettingsUser.Id
                        });

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                InputSagaWorkerEventsOutputEventTuple.SetClientSettings,
                InputSagaWorkerEventsOutputEventTuple.SetClientSettings.GenerateEventHeader(12),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string id) => id == InputSagaWorkerEventsOutputEventTuple.ClientSettingsUser.Id),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(
                                InputSagaWorkerEventsOutputEventTuple.ClientSettingsCalculatedResolved
                                    .CalculatedSettings)),
                    It.Is(
                        (string clientSettingsKey) => clientSettingsKey
                            == InputSagaWorkerEventsOutputEventTuple
                                .ClientSettingsCalculatedResolved
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
                        (string profileId) => profileId == InputSagaWorkerEventsOutputEventTuple.ClientSettingsUser.Id),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            List<EventTuple> result = sagaService.GetDictionary().Values.First();

            ((ProfileClientSettingsSet)result[0].Event).ClientSettings = ((ProfileClientSettingsSet)result[0].Event)
                .ClientSettings.Replace("\n", "")
                .Replace("\r", "")
                .Replace(" ", "");

            List<EventTuple> resolvedClientSettingsUserEventTuple = hops > 0
                ? InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsInheritedUserEventTuple
                : InputSagaWorkerEventsOutputEventTuple.ResolvedClientSettingsUserEventTuple;

            resolvedClientSettingsUserEventTuple.Should()
                                                .BeEquivalentTo(
                                                    result,
                                                    opt => opt.RespectingRuntimeTypes()
                                                              .Excluding(evt => evt.Event.EventId)
                                                              .Excluding(p => p.Event.MetaData.Timestamp)
                                                              .Excluding(p => p.Event.MetaData.Batch));
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
                        new FirstLevelProjectionUser
                        {
                            Id = _user.Id
                        });

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ProfileClientSettingsSetFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _eventObjectOnlyWithUser,
                _eventObjectOnlyWithUser.GenerateEventHeader(12),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.SetClientSettingsAsync(
                    It.Is((string id) => id == _user.Id),
                    It.Is(
                        (string clientSettingsValue) => ExtractNewLinesFromString(clientSettingsValue)
                            .Equals(_clientSettings.Values.First())),
                    It.Is((string clientSettingsKey) => clientSettingsKey == _clientSettings.Keys.First()),
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
                    It.Is((string profileId) => profileId == _user.Id),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
