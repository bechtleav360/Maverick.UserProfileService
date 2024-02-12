using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V3;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V3
{
    /// <summary>
    ///     Tests for <see cref="Handler.V2.OrganizationCreatedFirstLevelEventHandler" />
    /// </summary>
    public class FunctionCreatedHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly FunctionCreatedEvent _createdEventWithoutTags;
        private readonly FunctionCreatedEvent _createdEventWithTags;
        private readonly FirstLevelProjectionFunction _function;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCreatedHandlerTest" /> class.
        /// </summary>
        public FunctionCreatedHandlerTest()
        {
            _function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances().Single();
            List<TagAssignment> tagsAssignments = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true);

            _createdEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateV3FunctionCreatedEvent(_function);

            _createdEventWithTags =
                MockedSagaWorkerEventsBuilder.CreateV3FunctionCreatedEvent(_function, tagsAssignments.ToArray());

            _mockSagaService = MockProvider.GetDefaultMock<ISagaService>();
            _mockCreator = MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();
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
