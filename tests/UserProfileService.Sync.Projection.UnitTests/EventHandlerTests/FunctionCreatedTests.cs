using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using AutoMapper;
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
    public class FunctionCreatedTests
    {
        private readonly Mapper _mapper;

        public FunctionCreatedTests()
        {
            _mapper = new Mapper(new MapperConfiguration(conf => conf.AddProfile(new MappingProfiles())));

        }

        [Theory]
        [AutoData]
        public async Task Handle_message_should_work(FunctionCreated functionCreated)
        {
            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();

            profileServiceMock
                .Setup(p => p.CreateFunctionAsync(It.IsAny<FunctionSync>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FunctionSync f, CancellationToken _) => f);

            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); });

            var sut = ActivatorUtilities.CreateInstance<FunctionCreatedEventHandler>(services);
            var convertedFunction = _mapper.Map<FunctionSync>(functionCreated);

            // act
            await sut.HandleEventAsync(
                functionCreated.AddDefaultMetadata(services),
                functionCreated.GenerateEventHeader(14));

            // assert
            profileServiceMock.Verify(
                p => p.CreateFunctionAsync(
                    It.Is(convertedFunction,new ObjectComparer<FunctionSync>()),
                    It.IsAny<CancellationToken>()));

            profileServiceMock.Verify(
                p => p.TrySaveProjectionStateAsync(
                    It.IsAny<ProjectionState>(),It.IsAny<IDatabaseTransaction>(),It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>()));
        }
    }
}
