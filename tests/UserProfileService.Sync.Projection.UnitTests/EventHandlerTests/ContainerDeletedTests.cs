using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class ContainerDeletedTests
    {
        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(ContainerDeleted containerDeleted,
                                                     OrganizationSync childObject)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            containerDeleted.ContainerType = ContainerType.Organization;
            containerDeleted.ContainerId = Guid.NewGuid().ToString();
            childObject.Id = containerDeleted.MemberId;

            childObject.RelatedObjects = new List<ObjectRelation>
                                         {
                                             new ObjectRelation(
                                                 AssignmentType.ChildrenToParent,
                                                 new KeyProperties(containerDeleted.ContainerId, "external"),
                                                 containerDeleted.ContainerId,
                                                 ObjectType.Organization),
                                             new ObjectRelation(
                                                 AssignmentType.ParentsToChild,
                                                 new KeyProperties("123", "test"),
                                                 "B532679A-780F-419D-A871-3E6BAE02047D",
                                                 ObjectType.Organization)
                                         };

            OrganizationSync childCopy = CloningHelpers.CloneOrganizationSync(childObject);

            profileServiceMock.Setup(
                                  p => p.GetProfileAsync<OrganizationSync>(
                                      It.Is<string>(id => id == containerDeleted.MemberId),
                                      It.IsAny<CancellationToken>()))
                              .ReturnsAsync(childObject);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);
            containerDeleted.AddDefaultMetadata(services.GetRequiredService<IStreamNameResolver>(), ObjectType.Organization);

            containerDeleted.ContainerId =
                HandlerTestsPreparationHelper.GetRelatedProfileId(containerDeleted.MetaData.RelatedEntityId);

            StreamedEventHeader eventHeader = containerDeleted.GenerateEventHeader(
                14,
                $"{ProfileKind.Organization:G}#{containerDeleted.MemberId}");

            // act
            await sut.HandleEventAsync(
                containerDeleted,
                eventHeader);

            // assert
            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<OrganizationSync>(modified
                                                => AssertRelationOfChildOrgHasBeenChanged(modified,
                                                    childCopy,
                                                    containerDeleted.ContainerId)),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }

        [Theory]
        [AutoData]
        public async Task Handle_message_of_deleted_organization_with_invalid_member_should_fail(ContainerDeleted containerDeleted)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            containerDeleted.ContainerType = ContainerType.Organization;
            containerDeleted.ContainerId = Guid.NewGuid().ToString();

            profileServiceMock
                .Setup(
                    p => p.GetProfileAsync<OrganizationSync>(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(OrganizationSync));

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<ContainerDeletedEventHandler>(services);
            containerDeleted.AddDefaultMetadata(services.GetRequiredService<IStreamNameResolver>(), ObjectType.Organization);

            containerDeleted.ContainerId =
                HandlerTestsPreparationHelper.GetRelatedProfileId(containerDeleted.MetaData.RelatedEntityId);

            StreamedEventHeader eventHeader = containerDeleted.GenerateEventHeader(
                14,
                $"{ProfileKind.Organization:G}#{containerDeleted.MemberId}");

            // act
            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => sut.HandleEventAsync(
                    containerDeleted,
                    eventHeader));
        }

        private static bool AssertRelationOfChildOrgHasBeenChanged(
            OrganizationSync modifiedChild,
            OrganizationSync originalChildInstance,
            string oldParentId)
        {
            modifiedChild.Should().NotBeNull();

            modifiedChild.Should()
                         .BeEquivalentTo(originalChildInstance,
                                         options => options.Excluding(org => org.RelatedObjects)
                                                           .Excluding(org => org.ExternalIds));

            modifiedChild.RelatedObjects
                         .Should()
                         .NotContainEquivalentOf(
                             new ObjectRelation(
                                 AssignmentType.ChildrenToParent,
                                 null,
                                 oldParentId,
                                 ObjectType.Organization));

            return true;
        }
    }
}
