using AutoFixture.Xunit2;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using Maverick.UserProfileService.Models.EnumModels;
using System.Linq;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Projection.Abstractions;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = UserProfileService.Sync.Abstraction.Models.ProfileKind;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class WasAssignedToOrganizationTests
    {
        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(WasAssignedToOrganization wasAssignedToOrganization, OrganizationSync organization)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            OrganizationSync organizationForCompare = organization.CloneJson();
           
            organizationForCompare.RelatedObjects.Add(
                new ObjectRelation(
                    AssignmentType.ChildrenToParent,
                    organization.ExternalIds.FirstOrDefault(),
                    organization.Id,
                    ObjectType.Organization,
                    wasAssignedToOrganization.Conditions?.ToList()));

            wasAssignedToOrganization.ProfileId = organization.Id;

            profileServiceMock
                .Setup(p => p.GetProfileAsync<OrganizationSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<WasAssignedToOrganizationEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                wasAssignedToOrganization.AddDefaultMetadata(services),
                wasAssignedToOrganization.GenerateEventHeader(14,
                    $"{ProfileKind.Organization:G}#{wasAssignedToOrganization.ProfileId}"));

            // assert
            profileServiceMock.Verify(
                p => p.GetProfileAsync<OrganizationSync>(
                    It.Is<string>(id => id == wasAssignedToOrganization.ProfileId),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is(organizationForCompare, new ObjectComparer<OrganizationSync>(options => options.Excluding(m => m.RelatedObjects).Excluding(m => m.ExternalIds))),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<OrganizationSync>(o => o.RelatedObjects.Count == organizationForCompare.RelatedObjects.Count),
                    It.IsAny<CancellationToken>()));


            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(), It.IsAny<IDatabaseTransaction>(), It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
