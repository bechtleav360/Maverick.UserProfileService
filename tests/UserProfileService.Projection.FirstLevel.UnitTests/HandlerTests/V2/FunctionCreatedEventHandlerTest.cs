using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    /// <summary>
    ///     Tests for <see cref="FunctionCreatedFirstLevelEventHandler" />
    /// </summary>
    public class FunctionCreatedHandlerTest
    {
        private readonly FunctionCreatedEvent _createdEventWithoutTags;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCreatedHandlerTest" /> class.
        /// </summary>
        public FunctionCreatedHandlerTest()
        {
            FirstLevelProjectionFunction function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances().Single();

            _createdEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateV2FunctionCreatedEvent(function);
        }
        
        [Fact]
        public async Task Handler_should_work()
        {
            //arrange
            Mock<IDatabaseTransaction> transaction = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<FunctionCreatedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _createdEventWithoutTags,
                _createdEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.CreateFunctionAsync(
                    It.IsAny<FirstLevelProjectionFunction>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            sagaService.Verify();

            sagaService.Verify(
                s => s.ExecuteBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}
