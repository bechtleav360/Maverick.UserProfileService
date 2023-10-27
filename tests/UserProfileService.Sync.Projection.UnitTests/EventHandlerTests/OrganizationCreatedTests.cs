using AutoFixture.Xunit2;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Threading;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using UserProfileService.Sync.Utilities;
using Xunit;
using It = Moq.It;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class OrganizationCreatedTests
    {
        private readonly Mapper _mapper;

        public OrganizationCreatedTests()
        {
            _mapper = new Mapper(new MapperConfiguration(conf => conf.AddProfile(new MappingProfiles())));

        }

        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(OrganizationCreated organizationCreated)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();

            profileServiceMock
                .Setup(p => p.CreateProfileAsync(It.IsAny<OrganizationSync>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrganizationSync f, CancellationToken _) => f);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<OrganizationCreatedEventHandler>(services);
            var convertedFOrganization = _mapper.Map<OrganizationSync>(organizationCreated);

            // act
            await sut.HandleEventAsync(
                organizationCreated.AddDefaultMetadata(services),
                organizationCreated.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.CreateProfileAsync(
                    It.Is(convertedFOrganization, new ObjectComparer<OrganizationSync>()),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(), It.IsAny<IDatabaseTransaction>(), It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
