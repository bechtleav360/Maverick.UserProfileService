using AutoFixture.Xunit2;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using AgRangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class MemberRemovedTests
    {
        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(
          MemberRemoved memberDeleted,
          OrganizationSync organization,
          ObjectRelation relatedObject,
          AgRangeCondition rangeCondition)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            OrganizationSync organizationForCompare = organization.CloneJson();
           
            relatedObject.ObjectType = ObjectType.Organization;
            relatedObject.Conditions = new List<AgRangeCondition>{rangeCondition};
            organization.RelatedObjects.Add(relatedObject);
            memberDeleted.MemberId = relatedObject.MaverickId;
            memberDeleted.Conditions.Add(rangeCondition);

            profileServiceMock
                .Setup(p => p.GetProfileAsync<OrganizationSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<MemberRemovedEventHandler>(services);

            // act
            await sut.HandleEventAsync(
                memberDeleted.AddDefaultMetadata(
                    services,
                    new ObjectIdent(Guid.NewGuid().ToString(), ObjectType.Organization)),
                memberDeleted.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.GetProfileAsync<OrganizationSync>(
                    It.Is<string>(id => id == memberDeleted.ParentId),
                    It.IsAny<CancellationToken>()));

            // The test should in the future completed so that we can also compare the elements inside related objects (nested objects degree issue)
            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is(
                        organizationForCompare,
                        new ObjectComparer<OrganizationSync>(
                            options => options.Excluding(s => s.ExternalIds).Excluding(s => s.RelatedObjects))),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is<OrganizationSync>(
                       o => o.RelatedObjects.Count == organizationForCompare.RelatedObjects.Count),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),
                    It.IsAny<IDatabaseTransaction>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
