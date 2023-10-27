using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
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
using UserProfileService.Projection.FirstLevel.Implementation;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using ClientSettingsClientSettings = Maverick.UserProfileService.AggregateEvents.Resolved.V1.ClientSettingsCalculated;

namespace UserProfileService.Projection.FirstLevel.Tests.ImplementationTests
{
    public class TemporaryAssignmentsExecutorTests
    {
        // use ids to identify assignments that should have an error message
        private const string OldStateInactiveId = "Inactive-State-With-All-Possible-NotificationState";
        private const string OldStateNotProcessedId = "Not-Processed-State-With-Not-Possible-Combination";
        private const string OldStateActiveId = "Acitve-State-With-Not-Possible-Combination";
        private const string OldStateActiveWithExpiration = "AcitveWithExpiration-State-With-Not-Possible-Combination";
        private static ITestOutputHelper _helper;
        private readonly CancellationToken _cancellationToken;

        // clientSetting test group --> user will be active, group has a client settings
        private readonly ObjectIdent _groupClientSettings;
        private readonly ObjectIdent _groupLevelOneA;
        private readonly ObjectIdent _groupLevelOneB;
        private readonly ObjectIdent _groupLevelThree;
        private readonly ObjectIdent _groupLevelTwo;
        private readonly ObjectIdent _groupRoot;

        // not be used for assignment event tuple
        private readonly ObjectIdent _groupWillShouldBeFilteredOut;
        private readonly DateTime _start;
        private readonly IDatabaseTransaction _transaction;
        private readonly ObjectIdent _userClientSettings;

        private readonly ObjectIdent _userOne;
        private readonly ObjectIdent _userThree;
        private readonly ObjectIdent _userTwo;
        private readonly ObjectIdent _userWillShouldBeFilteredOut;

        public TemporaryAssignmentsExecutorTests(ITestOutputHelper helper)
        {
            // regarding sample tree -> see bottom
            _groupRoot = new ObjectIdent("grp-root", ObjectType.Group);
            _groupLevelOneA = new ObjectIdent("grp-level-one-a", ObjectType.Group);
            _groupLevelOneB = new ObjectIdent("grp-level-one-b", ObjectType.Group);
            _groupLevelTwo = new ObjectIdent("grp-level-two", ObjectType.Group);
            _groupLevelThree = new ObjectIdent("grp-level-three", ObjectType.Group);

            _userOne = new ObjectIdent("user-one", ObjectType.User);
            _userTwo = new ObjectIdent("user-two", ObjectType.User);
            _userThree = new ObjectIdent("user-three", ObjectType.User);

            // Only used for have an filled FirstLevelAssignment
            _groupWillShouldBeFilteredOut = new ObjectIdent("grp-filtered--used", ObjectType.Group);
            _userWillShouldBeFilteredOut = new ObjectIdent("user-filtered-out", ObjectType.User);

            _transaction = MockProvider.GetDefaultTransactionMock();
            _cancellationToken = new CancellationToken();
            _helper = helper;
            _start = DateTime.UtcNow;

            _groupClientSettings = new ObjectIdent("grp-for-client-settings", ObjectType.Group);
            _userClientSettings = new ObjectIdent("user-for-client-settings", ObjectType.User);
        }

        private Mock<IFirstLevelProjectionRepository> GetNewRepoMock()
        {
            Mock<IFirstLevelProjectionRepository> mock = MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            mock.Setup(r => r.StartTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction);

            mock.Setup(
                    r => r.GetTemporaryAssignmentsAsync(
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because the correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .ReturnsAsync(GetAssignments);

            mock.Setup(
                    r => r.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .ReturnsAsync(
                    (ObjectIdent ident, IDatabaseTransaction _, CancellationToken __) => GetChildren(ident.Id));

            mock.Setup(
                    r => r.UpdateTemporaryAssignmentStatesAsync(
                        It.IsAny<IList<FirstLevelProjectionTemporaryAssignment>>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .Returns(Task.CompletedTask);

            mock.Setup(
                    r => r.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<FirstLevelProjectionsClientSetting>());

            return mock;
        }

        private IList<FirstLevelProjectionsClientSetting> GetClientSettings()
        {
            return new List<FirstLevelProjectionsClientSetting>
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
            };
        }

        private IList<FirstLevelProjectionTemporaryAssignment> GetAssignmentForClientSettings()
        {
            return new List<FirstLevelProjectionTemporaryAssignment>
            {
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        "entry_group-level-two_to_group_level-one-b",
                    Start = DateTime.UtcNow.AddHours(-1),
                    End = DateTime.UtcNow.AddMonths(6),
                    LastModified =
                        DateTime.UtcNow.AddHours(-18),
                    TargetId = _groupClientSettings.Id, // parent
                    TargetType = _groupClientSettings.Type,
                    ProfileId = _userClientSettings.Id, // child
                    ProfileType = _userClientSettings.Type,
                    State = TemporaryAssignmentState
                        .NotProcessed,
                    NotificationStatus =
                        NotificationStatus.NoneSent
                }
            };
        }

        private IList<FirstLevelProjectionTemporaryAssignment> GetAssignments()
        {
            return new List<FirstLevelProjectionTemporaryAssignment>
            {
                // t.b. inactive (endDate was in the nearest future but now already past)
                // TemporaryAssignmentState: turns from activeWithExpiration --> inactive
                // NotificationStatus: turns from ActivationSent --> BothSends (includes DeactivationSend)
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id = "entry_user-one_to_group_level-one-a",
                    Start = null,
                    End = DateTime.UtcNow.AddMinutes(-20),
                    LastModified = DateTime.UtcNow.AddHours(-5),
                    TargetId = _groupLevelOneA.Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userOne.Id, // child
                    ProfileType = ObjectType.User,
                    State = TemporaryAssignmentState.ActiveWithExpiration,
                    NotificationStatus = NotificationStatus.ActivationSent
                },
                // t.b. active (startDate was in the nearest future but now already past AND endDate in the future)
                // TemporaryAssignmentState: turns from notProcessed --> activeWithExpiration
                // NotificationStatus: turns from noneSent --> ActivationSent
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id = "entry_group-level-two_to_group_level-one-b",
                    Start = DateTime.UtcNow.AddHours(-1),
                    End = DateTime.UtcNow.AddMonths(6),
                    LastModified = DateTime.UtcNow.AddHours(-18),
                    TargetId = _groupLevelTwo.Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _groupLevelOneB.Id, // child
                    ProfileType = ObjectType.Group,
                    State = TemporaryAssignmentState.NotProcessed,
                    NotificationStatus = NotificationStatus.NoneSent
                },
                // t.b. active, but forever (startDate was in the nearest future but now already past, endDate not given)
                // TemporaryAssignmentState: turns from notProcessed --> active 
                // NotificationStatus: turns from noneSent --> ActivationSent
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id = "entry_group-level-three_to_group_level-two",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupLevelThree.Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _groupLevelTwo.Id, // child
                    ProfileType = ObjectType.Group,
                    State = TemporaryAssignmentState.NotProcessed,
                    NotificationStatus = NotificationStatus.NoneSent
                }
                // states that are not process and get an error message
            };
        }

        private IList<FirstLevelProjectionTemporaryAssignment> GetAllCorruptedAssignments()
        {
            return new List<FirstLevelProjectionTemporaryAssignment>
            {
                // Old state is inactive with all possible notification state

                // all states with inactive
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateInactiveId}-01",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.NoneSent,
                    State = TemporaryAssignmentState.Inactive
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateInactiveId}-02",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus =
                        NotificationStatus.ActivationSent,
                    State = TemporaryAssignmentState.Inactive
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateInactiveId} -03",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus =
                        NotificationStatus.DeactivationSent,
                    State = TemporaryAssignmentState.Inactive
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateInactiveId} -04",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.BothSent,
                    State = TemporaryAssignmentState.Inactive
                },

                // all impossible states with NotProcessed
                // old state not processed => active
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateNotProcessedId}-01",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.ActivationSent,
                    State = TemporaryAssignmentState.NotProcessed
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateNotProcessedId}-02",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.BothSent,
                    State = TemporaryAssignmentState.NotProcessed
                },

                // old state not process => new state activeWithExpiration
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateNotProcessedId}-03",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = DateTime.UtcNow.AddHours(12),
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.ActivationSent,
                    State = TemporaryAssignmentState.NotProcessed
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateNotProcessedId}-04",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = DateTime.UtcNow.AddHours(12),
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.BothSent,
                    State = TemporaryAssignmentState.NotProcessed
                },

                // all possible combination with state: Active
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveId}-01",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.NoneSent,
                    State = TemporaryAssignmentState.Active
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveId}-02",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.ActivationSent,
                    State = TemporaryAssignmentState.Active
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveId}-03",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.DeactivationSent,
                    State = TemporaryAssignmentState.Active
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveId}-04",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = null,
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.BothSent,
                    State = TemporaryAssignmentState.Active
                },
                // all impossible combination with activeWithExpiration become inactive
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveWithExpiration}-01",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = DateTime.UtcNow.AddMinutes(-12),
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.BothSent,
                    State = TemporaryAssignmentState.ActiveWithExpiration
                },
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id =
                        $"{OldStateActiveWithExpiration}-02",
                    Start = DateTime.UtcNow.AddMinutes(-10),
                    End = DateTime.UtcNow.AddMinutes(-12),
                    LastModified = DateTime.UtcNow.AddDays(-7),
                    TargetId = _groupWillShouldBeFilteredOut
                        .Id, // parent
                    TargetType = ObjectType.Group,
                    ProfileId = _userWillShouldBeFilteredOut
                        .Id, // child
                    ProfileType = ObjectType.Group,
                    NotificationStatus = NotificationStatus.DeactivationSent,
                    State = TemporaryAssignmentState.ActiveWithExpiration
                }
            };
        }

        private IList<FirstLevelRelationProfile> GetChildren(string id)
        {
            //           grp-root
            //           /       \
            //  grp-level-one-a   grp-level-one-b
            // (t.b. inactive) \        |        \ (to become active)
            //               user-one  user-two  grp-level-two
            //                                   /       \ (to become active, but forever)
            //                           user-three     grp-level-three

            if (id == _userOne.Id
                || id == _userTwo.Id
                || id == _userThree.Id)
            {
                return new List<FirstLevelRelationProfile>();
            }

            if (id == _groupRoot.Id)
            {
                return new List<FirstLevelRelationProfile>
                       {
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_groupLevelOneA.Id),
                               FirstLevelMemberRelation.DirectMember),
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_groupLevelOneB.Id),
                               FirstLevelMemberRelation.DirectMember)
                       };
            }

            if (id == _groupLevelOneA.Id)
            {
                return new List<FirstLevelRelationProfile>
                       {
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionUserWithId(_userOne.Id),
                               FirstLevelMemberRelation.DirectMember)
                       };
            }

            if (id == _groupLevelOneB.Id)
            {
                return new List<FirstLevelRelationProfile>
                       {
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_userTwo.Id),
                               FirstLevelMemberRelation.DirectMember),
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_groupLevelTwo.Id),
                               FirstLevelMemberRelation.DirectMember)
                       };
            }

            if (id == _groupLevelTwo.Id)
            {
                return new List<FirstLevelRelationProfile>
                       {
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionUserWithId(_userThree.Id),
                               FirstLevelMemberRelation.DirectMember),
                           new FirstLevelRelationProfile(
                               MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(_groupLevelThree.Id),
                               FirstLevelMemberRelation.DirectMember)
                       };
            }

            if (id == _groupLevelThree.Id)
            {
                return new List<FirstLevelRelationProfile>();
            }

            throw new XunitException($"Wrong id. This should not happening. Got {id}");
        }

        // new inactive assignments (parent group-level-one-a to child user-one)
        private IEnumerable<EventTuple> GetExpectedTuplesOfNewInactiveAssignedSubTree(IServiceProvider services)
        {
            var streamNameResolver = ServiceProviderServiceExtensions.GetRequiredService<IStreamNameResolver>(services);

            var newActiveAssignment = new AssignmentConditionTriggered
            {
                ProfileId = _userOne.Id,
                TargetId = _groupLevelOneA.Id,
                TargetObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                IsActive = false
            };

            // old parent group
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelOneA),
                newActiveAssignment);

            // old child - only one user
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_userOne),
                newActiveAssignment);
        }

        // new active assignments (parent group-level-one-b to child group-level-two)
        private IEnumerable<EventTuple> GetExpectedTuplesOfNewActiveAssignedSubTree(IServiceProvider services)
        {
            var streamNameResolver = ServiceProviderServiceExtensions.GetRequiredService<IStreamNameResolver>(services);

            var newActiveAssignment = new AssignmentConditionTriggered
            {
                ProfileId = _groupLevelTwo.Id,
                TargetId = _groupLevelOneB.Id,
                TargetObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                IsActive = true
            };

            // new parent group
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelOneB),
                newActiveAssignment);

            // new child - user and group
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelTwo),
                newActiveAssignment);

            // children of new child
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelThree),
                newActiveAssignment);

            yield return new EventTuple(
                streamNameResolver.GetStreamName(_userThree),
                newActiveAssignment);
        }

        private IEnumerable<EventTuple> GetExpectedTuplesForClientSettings(IServiceProvider provider)
        {
            var streamNameResolver = ServiceProviderServiceExtensions.GetService<IStreamNameResolver>(provider);
            FirstLevelProjectionsClientSetting clientSettings = GetClientSettings().First();

            var activeAssignment = new AssignmentConditionTriggered
            {
                ProfileId = _userClientSettings.Id,
                TargetId = _groupClientSettings.Id,
                TargetObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                IsActive = true
            };

            yield return new EventTuple(
                streamNameResolver.GetStreamName(_userClientSettings),
                activeAssignment);

            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupClientSettings),
                activeAssignment);

            yield return new EventTuple(
                streamNameResolver.GetStreamName(_userClientSettings),
                new ClientSettingsInvalidated
                {
                    Keys = new[] { clientSettings.SettingsKey },
                    ProfileId = _userClientSettings.Id
                });

            yield return new EventTuple(
                streamNameResolver.GetStreamName(_userClientSettings),
                new ClientSettingsClientSettings
                {
                    Key = clientSettings.SettingsKey,
                    CalculatedSettings = clientSettings.Value,
                    ProfileId = _userClientSettings.Id
                });
        }

        // new active assignments (parent group-level-two to child group-level-three)
        private IEnumerable<EventTuple> GetExpectedTuplesOfNewForeverActiveAssignedSubTree(IServiceProvider services)
        {
            var streamNameResolver = ServiceProviderServiceExtensions.GetRequiredService<IStreamNameResolver>(services);

            var newActiveAssignment = new AssignmentConditionTriggered
            {
                ProfileId = _groupLevelThree.Id,
                TargetId = _groupLevelTwo.Id,
                TargetObjectType = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                IsActive = true
            };

            // new parent group
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelTwo),
                newActiveAssignment);

            // new child - user and group
            yield return new EventTuple(
                streamNameResolver.GetStreamName(_groupLevelThree),
                newActiveAssignment);
        }

        private static EquivalencyAssertionOptions<EventTuple> IgnoreMetadataAndEventId(
            EquivalencyAssertionOptions<EventTuple> arg)
        {
            return arg.Excluding(
                info => info.Path
                        .Contains(".MetaData.")
                    || info.Path
                        .Contains(".EventId"));
        }

        private static bool CheckAllAssignmentsForErrors(
            IList<FirstLevelProjectionTemporaryAssignment> temporaryAssignments)
        {
            if (temporaryAssignments.Count == 0)
            {
                return false;
            }

            if (temporaryAssignments.Any(all => all.State != TemporaryAssignmentState.ErrorOccurred))
            {
                FirstLevelProjectionTemporaryAssignment wrongEntity =
                    temporaryAssignments.First(tmp => tmp.State != TemporaryAssignmentState.ErrorOccurred);

                _helper.WriteLine($"The entity with the wrong id: {wrongEntity.Id}");

                return false;
            }

            if (temporaryAssignments.Any(tmp => string.IsNullOrEmpty(tmp.LastErrorMessage)))
            {
                FirstLevelProjectionTemporaryAssignment wrongEntity =
                    temporaryAssignments.First(tmp => string.IsNullOrEmpty(tmp.LastErrorMessage));

                _helper.WriteLine($"The entity with the wrong id: {wrongEntity.Id}");

                return false;
            }

            return true;
        }

        private static bool CheckIfStatesChangedRight(
            IList<FirstLevelProjectionTemporaryAssignment> temporaryAssignments)
        {
            if (temporaryAssignments.ElementAt(0).State != TemporaryAssignmentState.Inactive
                || temporaryAssignments.ElementAt(0).NotificationStatus != NotificationStatus.BothSent)
            {
                return false;
            }

            if (temporaryAssignments.ElementAt(1).State != TemporaryAssignmentState.ActiveWithExpiration
                || temporaryAssignments.ElementAt(1).NotificationStatus != NotificationStatus.ActivationSent)
            {
                return false;
            }

            if (temporaryAssignments.ElementAt(2).State != TemporaryAssignmentState.Active
                || temporaryAssignments.ElementAt(2).NotificationStatus != NotificationStatus.ActivationSent)
            {
                return false;
            }

            return true;
        }

        [Fact]
        public async Task Check_temporary_assignments_should_work()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();
            var sagaService = new MockSagaService();
            Mock<IFirstLevelProjectionRepository> repoMock = GetNewRepoMock();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => ServiceCollectionServiceExtensions.AddSingleton(
                    ServiceCollectionServiceExtensions.AddSingleton<TemporaryAssignmentsExecutor>(
                        ServiceCollectionServiceExtensions.AddSingleton<ISagaService>(
                            ServiceCollectionServiceExtensions.AddSingleton(
                                ServiceCollectionServiceExtensions.AddSingleton(s, streamNameResolverMock.Object),
                                repoMock.Object),
                            sagaService)),
                    MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>().Object));

            var sut = ServiceProviderServiceExtensions.GetRequiredService<TemporaryAssignmentsExecutor>(services);

            // act
            await sut.CheckTemporaryAssignmentsAsync(_cancellationToken);

            // assert
            repoMock.Verify(
                r => r.GetAllChildrenAsync(
                    It.Is<ObjectIdent>(ident => ident.Id == _groupLevelOneA.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoMock.Verify(
                r => r.GetAllChildrenAsync(
                    It.Is<ObjectIdent>(ident => ident.Id == _groupLevelTwo.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            repoMock.Verify(
                r => r.GetAllChildrenAsync(
                    It.Is<ObjectIdent>(ident => ident.Id == _groupLevelThree.Id),
                    ItShould.BeEquivalentTo(_transaction),
                    ItShould.BeEquivalentTo(_cancellationToken)),
                Times.Once);

            repoMock.Verify(
                r => r.UpdateTemporaryAssignmentStatesAsync(
                    It.Is<IList<FirstLevelProjectionTemporaryAssignment>>(ass => CheckIfStatesChangedRight(ass)),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            IReadOnlyDictionary<Guid, List<EventTuple>> createdTuples = sagaService.GetDictionary();
            Assert.NotEmpty(createdTuples);
            Assert.Equal(3, createdTuples.Count);

            createdTuples.First()
                .Value
                .Should()
                .BeEquivalentTo(
                    GetExpectedTuplesOfNewInactiveAssignedSubTree(services),
                    IgnoreMetadataAndEventId);

            createdTuples.ElementAt(1)
                .Value
                .Should()
                .BeEquivalentTo(
                    GetExpectedTuplesOfNewActiveAssignedSubTree(services),
                    IgnoreMetadataAndEventId);

            createdTuples.ElementAt(2)
                .Value
                .Should()
                .BeEquivalentTo(
                    GetExpectedTuplesOfNewForeverActiveAssignedSubTree(services),
                    IgnoreMetadataAndEventId);
        }

        [Fact]
        public async Task Check_temporary_assignments_invalid_cases_have_error_message()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();
            var sagaService = new MockSagaService();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            repoMock.Setup(r => r.StartTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction);

            repoMock.Setup(
                    r => r.GetTemporaryAssignmentsAsync(
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because the correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .ReturnsAsync(GetAllCorruptedAssignments);

            repoMock.Setup(
                    r => r.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                    .ReturnsAsync(() => new List<FirstLevelRelationProfile>());

            repoMock.Setup(
                    r => r.UpdateTemporaryAssignmentStatesAsync(
                        It.IsAny<IList<FirstLevelProjectionTemporaryAssignment>>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    r => r.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new List<FirstLevelProjectionsClientSetting>());

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => ServiceCollectionServiceExtensions.AddSingleton(
                    ServiceCollectionServiceExtensions.AddSingleton<TemporaryAssignmentsExecutor>(
                        ServiceCollectionServiceExtensions.AddSingleton<ISagaService>(
                            ServiceCollectionServiceExtensions.AddSingleton(
                                ServiceCollectionServiceExtensions.AddSingleton(s, streamNameResolverMock.Object),
                                repoMock.Object),
                            sagaService)),
                    MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>().Object));

            var sut = ServiceProviderServiceExtensions.GetRequiredService<TemporaryAssignmentsExecutor>(services);

            // act
            await sut.CheckTemporaryAssignmentsAsync(_cancellationToken);

            // Check corrupted states.
            repoMock.Verify(
                r => r.UpdateTemporaryAssignmentStatesAsync(
                    It.Is<IList<FirstLevelProjectionTemporaryAssignment>>(ass => CheckAllAssignmentsForErrors(ass)),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Check_recalculating_clientSettings_when_conditional_assignment_is_active()
        {
            // arrange
            Mock<IStreamNameResolver> streamNameResolverMock = MockProvider.GetDefaultMock<IStreamNameResolver>();
            var sagaService = new MockSagaService();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            repoMock.Setup(r => r.StartTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction);

            repoMock.Setup(
                    r => r.GetTemporaryAssignmentsAsync(
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because the correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .ReturnsAsync(GetAssignmentForClientSettings);

            repoMock.Setup(
                    r => r.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                    .ReturnsAsync(() => new List<FirstLevelRelationProfile>());

            repoMock.Setup(
                    r => r.UpdateTemporaryAssignmentStatesAsync(
                        It.IsAny<IList<FirstLevelProjectionTemporaryAssignment>>(),
                        ItShould.BeEquivalentTo(
                            _transaction,
                            "because correct transaction object must be set",
                            opt => opt.RespectingRuntimeTypes()),
                        ItShould.BeEquivalentTo(
                            _cancellationToken,
                            "because this is the correct cancellation token")))
                .Returns(Task.CompletedTask);

            repoMock.Setup(
                    r => r.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetClientSettings);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => ServiceCollectionServiceExtensions.AddSingleton(
                    ServiceCollectionServiceExtensions.AddSingleton<TemporaryAssignmentsExecutor>(
                        ServiceCollectionServiceExtensions.AddSingleton<ISagaService>(
                            ServiceCollectionServiceExtensions.AddSingleton(
                                ServiceCollectionServiceExtensions.AddSingleton(s, streamNameResolverMock.Object),
                                repoMock.Object),
                            sagaService)),
                    MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>().Object));

            var sut = ServiceProviderServiceExtensions.GetRequiredService<TemporaryAssignmentsExecutor>(services);

            await sut.CheckTemporaryAssignmentsAsync(_cancellationToken);

            List<EventTuple> result = sagaService.GetDictionary().Values.First();

            result.Should().BeEquivalentTo(GetExpectedTuplesForClientSettings(services), IgnoreMetadataAndEventId);
        }
    }
}
