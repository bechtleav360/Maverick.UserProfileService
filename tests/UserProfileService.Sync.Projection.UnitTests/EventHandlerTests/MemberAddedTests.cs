using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Threading;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using It = Moq.It;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class MemberAddedTests
    {
        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(MemberAdded memberAdded, OrganizationSync organization)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();
            memberAdded.ParentType = ContainerType.Organization;

            profileServiceMock
                .Setup(p => p.GetProfileAsync<OrganizationSync>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organization);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<MemberAddedEventHandler>(services);
            
            // act
            await sut.HandleEventAsync(
                memberAdded.AddDefaultMetadata(services),
                memberAdded.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.GetProfileAsync<OrganizationSync>(
                    It.Is<string>(id => id == memberAdded.ParentId),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.UpdateProfileAsync(
                    It.Is(organization,new ObjectComparer<OrganizationSync>()),
                    It.IsAny<CancellationToken>()));


            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(), It.IsAny<IDatabaseTransaction>(), It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
