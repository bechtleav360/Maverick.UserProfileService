using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;
using static UserProfileService.Projection.FirstLevel.Tests.InputSagaWorkerEventsOutputEventTuple;
using ResolvedTagAssignments = Maverick.UserProfileService.AggregateEvents.Common.Models.TagAssignment;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class ObjectAssignmentEventHandlerTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ObjectAssignmentEventHandlerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private bool CheckIdInListIds(List<string> ids, List<string> expectedIds)
        {
            return ids.All(expectedIds.Contains) && expectedIds.All(ids.Contains);
        }

        private async IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> GetDifference(
            IEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> results)
        {
            await Task.Yield();

            foreach (FirstLevelProjectionParentsTreeDifferenceResult result in results)
            {
                yield return result;
            }
        }

        private Mock<IFirstLevelProjectionRepository> GetRepository(
            MockDatabaseTransaction transaction)
        {
            var mock = new Mock<IFirstLevelProjectionRepository>();

            mock.ApplyWorkingTransactionSetup(transaction);

            mock.SetReturnsDefault(Task.CompletedTask);

            return mock;
        }

        // Test includes to add root to the firstLevel group.
        // The firstLevelGroup is already assigned to the secondLevelGroup
        //
        // Is state: RootGroup    FirstLevelGroup
        //                                |
        //                        SecondLevelGroup
        //
        // Desired state:              RootGroup
        //                                |
        //                          FirstLevelGroup
        //                                |
        //                          SecondLevelGroup
        [Fact]
        public async Task Handler_should_work_profile_assignments_group_to_group()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            var groups = new List<FirstLevelProjectionGroup>
            {
                RootGroup,
                FirstLevelGroup,
                SecondLevelGroup
            };

            repoMock.Setup(
                    opt => opt.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction database, CancellationToken cancellation)
                        => groups.FirstOrDefault(group => group.Id == id));

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(new List<FirstLevelProjectionsClientSetting>());

            repoMock.Setup(
                    opt => opt.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent parent, IDatabaseTransaction trans, CancellationToken token)
                        => parent.Id == RootGroup.Id
                            ? new List<FirstLevelRelationProfile>()
                            : new List<FirstLevelRelationProfile>
                              {
                                  new FirstLevelRelationProfile
                                  {
                                      Profile = SecondLevelGroup,
                                      Relation = FirstLevelMemberRelation.DirectMember
                                  }
                              });

            repoMock.Setup(
                    opt => opt.GetDifferenceInParentsTreesAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(
                    () => GetDifference(
                        new[]
                        {
                            new FirstLevelProjectionParentsTreeDifferenceResult
                            {
                                Profile = RootGroup,
                                ReferenceProfileId = FirstLevelGroup.Id,
                                MissingRelations = new List<FirstLevelProjectionTreeEdgeRelation>(),
                                ProfileTags = TagAssignmentGroup
                            },
                            new FirstLevelProjectionParentsTreeDifferenceResult
                            {
                                Profile = RootGroup,
                                ReferenceProfileId = SecondLevelGroup.Id,
                                MissingRelations = new List<FirstLevelProjectionTreeEdgeRelation>(),
                                ProfileTags = TagAssignmentGroup
                            }
                        }));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ObjectAssignmentFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                AddRootToFirstLevelGroupAssignment,
                AddRootToFirstLevelGroupAssignment.GenerateEventHeader(10));

            // Get result for the assignments
            IEnumerable<EventTuple> result = sagaService.GetDictionary().First().Value;

            result
                .Should()
                .BeEquivalentTo(
                    ResolvedEventsGroupsAssignments,
                    opt => opt.Excluding(p => p.Event.EventId)
                        .Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .RespectingRuntimeTypes());
        }

        // Test includes to add root to the firstLevel organization.
        // The firstLevelOrganization is already assigned to the secondLevelOrganization
        //
        // Is state: RootOrg   FirstLevelOrganization
        //                           |
        //                    SecondLevelOrganization
        //
        // Desired state:         RootOrg
        //                           |
        //                FirstLevelOrganization
        //                           |
        //                SecondLevelOrganization
        [Fact]
        public async Task Handler_should_work_profile_assignments_organization_to_organization()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            var organizations = new List<FirstLevelProjectionOrganization>
            {
                RootOrganization,
                FirstLevelOrganization,
                SecondLevelOrganization
            };

            repoMock.Setup(
                    opt => opt.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction database, CancellationToken cancellation)
                        => organizations.FirstOrDefault(org => org.Id == id));

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(new List<FirstLevelProjectionsClientSetting>());

            repoMock.Setup(
                    opt => opt.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent parent, IDatabaseTransaction trans, CancellationToken token) =>
                        parent.Id == RootOrganization.Id
                            ? new List<FirstLevelRelationProfile>()
                            : new List<FirstLevelRelationProfile>
                              {
                                  new FirstLevelRelationProfile
                                  {
                                      Profile = SecondLevelOrganization,
                                      Relation = FirstLevelMemberRelation.DirectMember
                                  }
                              });

            repoMock.Setup(
                    opt => opt.GetDifferenceInParentsTreesAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .Returns(
                    () => GetDifference(
                        new[]
                        {
                            new FirstLevelProjectionParentsTreeDifferenceResult
                            {
                                Profile = RootOrganization,
                                ReferenceProfileId = FirstLevelOrganization.Id,
                                MissingRelations = new List<FirstLevelProjectionTreeEdgeRelation>(),
                                ProfileTags = TagAssignmentOrganization
                            },
                            new FirstLevelProjectionParentsTreeDifferenceResult
                            {
                                Profile = RootOrganization,
                                ReferenceProfileId = SecondLevelOrganization.Id,
                                MissingRelations = new List<FirstLevelProjectionTreeEdgeRelation>(),
                                ProfileTags = TagAssignmentOrganization
                            }
                        }));

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ObjectAssignmentFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                AddRootToFirstLevelOrganizationAssignment,
                AddRootToFirstLevelGroupAssignment.GenerateEventHeader(10));

            IEnumerable<EventTuple> result = sagaService.GetDictionary().First().Value;

            result.Should()
                .BeEquivalentTo(
                    ResolvedEventsOrganizationsAssignments,
                    opt => opt.Excluding(p => p.Event.EventId)
                        .Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .RespectingRuntimeTypes());
        }

        // This test is a little be complex. We will assign a function to FirstGroup .
        // The FirstGroup has an assignment to the SecondGroup. The SecondGroup has FirstUser as member.
        // 
        // Is state: RootFunction   FirstGroup
        //                               |
        //                          SecondGroup
        //                               |
        //                           FirstUser
        //
        // Desired state:        RootFunction
        //                           |
        //                       FirstGroup
        //                           |
        //                       SecondGroup
        //                           |
        //                       FirstUser

        [Fact]
        public async Task Handler_should_work_profile_assignments_function_to_group()
        {
            //arrange
            var transaction = new MockDatabaseTransaction();
            Mock<IFirstLevelProjectionRepository> repoMock = GetRepository(transaction);
            var sagaService = new MockSagaService();

            var profiles = new List<IFirstLevelProjectionProfile>
            {
                FirstGroup,
                SecondGroup,
                FirstUser
            };

            repoMock.Setup(
                    opt => opt.GetProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (string id, IDatabaseTransaction database, CancellationToken cancellation)
                        => profiles.FirstOrDefault(group => group.Id == id));

            repoMock.Setup(
                    repo => repo.GetCalculatedClientSettingsAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        CancellationToken.None))
                .ReturnsAsync(new List<FirstLevelProjectionsClientSetting>());

            repoMock.Setup(
                    opt => opt.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent parent, IDatabaseTransaction trans, CancellationToken token) =>
                        new List<FirstLevelRelationProfile>
                        {
                            new FirstLevelRelationProfile
                            {
                                Profile = SecondGroup,
                                Relation = FirstLevelMemberRelation.DirectMember
                            },
                            new FirstLevelRelationProfile
                            {
                                Profile = FirstUser,
                                Relation = FirstLevelMemberRelation.IndirectMember
                            }
                        });

            repoMock.Setup(
                    opt => opt.GetFunctionAsync(
                        It.IsAny<string>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => RootFunction);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton<ISagaService>(sagaService);
                });

            var sut = ActivatorUtilities.CreateInstance<ObjectAssignmentFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                AddRootToFunctionToGroupAssignment,
                AddRootToFirstLevelGroupAssignment.GenerateEventHeader(10));

            IEnumerable<EventTuple> result = sagaService.GetDictionary().First().Value;

            result.Should()
                .BeEquivalentTo(
                    ResolvedEventsFunctionToGroupAssignments,
                    opt => opt.Excluding(p => p.Event.EventId)
                        .Excluding(p => p.Event.MetaData.Batch)
                        .Excluding(p => p.Event.MetaData.Timestamp)
                        .RespectingRuntimeTypes());
        }
    }
}
