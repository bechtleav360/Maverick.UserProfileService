using AutoFixture.Xunit2;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using UserProfileService.Sync.Utilities;
using Xunit;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class UserCreatedTests
    {
        private readonly Mapper _mapper;

        public UserCreatedTests()
        {
            _mapper = new Mapper(new MapperConfiguration(conf => conf.AddProfile(new MappingProfiles())));

        }
      
        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(UserCreated userCreated)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();

            profileServiceMock
                .Setup(p => p.CreateProfileAsync(It.IsAny<UserSync>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserSync f, CancellationToken _) => f);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<UserCreatedEventHandler>(services);
            var convertedFunction = _mapper.Map<UserSync>(userCreated);

            // act
            await sut.HandleEventAsync(
                userCreated.AddDefaultMetadata(services),
                userCreated.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.CreateProfileAsync(
                    It.Is(convertedFunction, new ObjectComparer<UserSync>()),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(), It.IsAny<IDatabaseTransaction>(), It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
