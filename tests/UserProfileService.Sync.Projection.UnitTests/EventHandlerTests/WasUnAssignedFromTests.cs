using AutoFixture.Xunit2;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = UserProfileService.Sync.Abstraction.Models.ProfileKind;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class WasUnAssignedFromTests
    {
        [Theory]
        [InlineAutoData(true)]
        [InlineAutoData(false)]
        public async Task Handle_message_should_work(bool multipleConditions,
                                                     WasUnassignedFrom wasUnAssignedToOrganization,
                                                     OrganizationSync parentOrganization,
                                                     OrganizationSync childOrganization)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            OrganizationSync organizationForCompare = CloningHelpers.CloneOrganizationSync(childOrganization);

            wasUnAssignedToOrganization.ParentType = ContainerType.Organization;
            wasUnAssignedToOrganization.ParentId = parentOrganization.Id;
            RangeCondition firstCondition = wasUnAssignedToOrganization.Conditions.FirstOrDefault();
            wasUnAssignedToOrganization.Conditions = new[] { firstCondition };
            wasUnAssignedToOrganization.ChildId = parentOrganization.RelatedObjects.FirstOrDefault()?.MaverickId;

            var staticCondition = new RangeCondition
                                  {
                                      Start = new DateTime(2000, 11, 3, 8, 40, 56, DateTimeKind.Utc),
                                      End = new DateTime(2001, 11, 3, 8, 40, 56, DateTimeKind.Utc)
                                  };

            var childRelation = new ObjectRelation(
                AssignmentType.ChildrenToParent,
                new KeyProperties("parent", "test"),
                parentOrganization.Id,
                ObjectType.Organization,
                new List<RangeCondition>
                {
                    firstCondition
                }.AddConditionally(
                    staticCondition,
                    multipleConditions));

            organizationForCompare.RelatedObjects
                                      .AddConditionally(childRelation, multipleConditions);

            parentOrganization.RelatedObjects.Add(
                new ObjectRelation(
                    AssignmentType.ParentsToChild,
                    new KeyProperties("child", "test"),
                    childOrganization.Id,
                    ObjectType.Organization,
                    new List<RangeCondition>
                        {
                            firstCondition
                        }
                        .AddConditionally(
                            staticCondition,
                            multipleConditions)));

            childOrganization.RelatedObjects.Add(
                childRelation);

            profileServiceMock
                .Setup(p => p.GetProfileAsync<OrganizationSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(childOrganization);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<WasUnassignedFromEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                wasUnAssignedToOrganization.AddDefaultMetadata(services),
                wasUnAssignedToOrganization.GenerateEventHeader(
                    14,
                    $"{ProfileKind.Organization:G}#{wasUnAssignedToOrganization.ChildId}"));

            // assert
            profileServiceMock.Verify(
                p => p.GetProfileAsync<OrganizationSync>(
                    It.Is<string>(id => id == wasUnAssignedToOrganization.ChildId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<OrganizationSync>(org => AssertUpdatedProfileCorrectly(org, organizationForCompare)),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        }

        private static bool AssertUpdatedProfileCorrectly(
            OrganizationSync objectToBeChecked,
            OrganizationSync referenceOrganization)
        {
            objectToBeChecked.Should()
                             .BeEquivalentTo(
                                 referenceOrganization,
                                 options => options.Excluding(o => o.ExternalIds)
                                                   .Excluding(o => o.RelatedObjects));

            objectToBeChecked.RelatedObjects.Should().NotBeNull();
            objectToBeChecked.RelatedObjects.Should().HaveCount(referenceOrganization.RelatedObjects.Count);

            return true;
        }
    }
}
