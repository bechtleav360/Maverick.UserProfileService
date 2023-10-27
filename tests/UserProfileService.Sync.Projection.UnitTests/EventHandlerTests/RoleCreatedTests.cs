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
    public class RoleCreatedTests
    {
        private readonly Mapper _mapper;

        public RoleCreatedTests()
        {
            _mapper = new Mapper(new MapperConfiguration(conf => conf.AddProfile(new MappingProfiles())));

        }

        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(RoleCreated roleCreated)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();

            profileServiceMock
                .Setup(p => p.CreateRoleAsync(It.IsAny<RoleSync>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleSync f, CancellationToken _) => f);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<RoleCreatedEventHandler>(services);
            var convertedFunction = _mapper.Map<RoleSync>(roleCreated);

            // act
            await sut.HandleEventAsync(
                roleCreated.AddDefaultMetadata(services),
                roleCreated.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.CreateRoleAsync(
                    It.Is(convertedFunction, new ObjectComparer<RoleSync>()),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(), It.IsAny<IDatabaseTransaction>(), It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
