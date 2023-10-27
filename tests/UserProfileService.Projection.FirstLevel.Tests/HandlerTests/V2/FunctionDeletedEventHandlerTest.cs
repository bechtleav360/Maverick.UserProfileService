using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Handler.V2;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class FunctionDeletedEventHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly FunctionDeletedEvent _deletedEventWithoutTags;
        private readonly FirstLevelProjectionFunction _function;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionDeletedEventHandlerTest" /> class.
        /// </summary>
        public FunctionDeletedEventHandlerTest()
        {
            _function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances().Single();

            _deletedEventWithoutTags =
                MockedSagaWorkerEventsBuilder.CreateFunctionDeletedEvent(_function);

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
            Mock<IStreamNameResolver> streamResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();
            var profiles = new List<IFirstLevelProjectionProfile>();
            profiles.AddRange(MockDataGenerator.GenerateFirstLevelProjectionUser(5));
            profiles.AddRange(MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances(2));

            repoMock.Setup(
                    x => x.GetAllChildrenAsync(
                        It.IsAny<ObjectIdent>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (ObjectIdent ident, IDatabaseTransaction transaction, CancellationToken ct) =>
                    {
                        var results = new List<IFirstLevelProjectionProfile>();
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionUser(5));
                        results.AddRange(MockDataGenerator.GenerateFirstLevelProjectionOrganizationInstances(2));

                        List<FirstLevelRelationProfile> firstLevelRelation = results.Select(
                                profile => new FirstLevelRelationProfile
                                           {
                                               Profile = profile,
                                               Relation = FirstLevelMemberRelation.DirectMember
                                           })
                            .ToList();
                        
                        // TODO add groups?
                        foreach (IFirstLevelProjectionProfile result in results)
                        {
                            sagaService.Setup(
                                    x => x.AddEventsAsync(
                                        It.IsAny<Guid>(),
                                        It.Is<IEnumerable<EventTuple>>(
                                            et => et.Any(
                                                t => t.TargetStream
                                                    == streamResolver.Object.GetStreamName(result.ToObjectIdent()))),
                                        It.IsAny<CancellationToken>()))
                                .Verifiable();
                        }

                        return firstLevelRelation;
                    });

            repoMock.Setup(
                    x => x.GetFunctionAsync(
                        It.Is<string>(id => id == _function.Id),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => _function);

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                {
                    s.AddSingleton(repoMock.Object);
                    s.AddSingleton(transaction.Object);
                    s.AddSingleton(sagaService.Object);
                });

            var sut = ActivatorUtilities.CreateInstance<FunctionDeletedFirstLevelEventHandler>(services);

            await sut.HandleEventAsync(
                _deletedEventWithoutTags,
                _deletedEventWithoutTags.GenerateEventHeader(10),
                CancellationToken.None);

            repoMock.Verify(
                repo => repo.DeleteFunctionAsync(
                    It.Is<string>(id => id == _function.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetFunctionAsync(
                    It.Is<string>(id => id == _function.Id),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Once);

            repoMock.Verify(
                repo => repo.GetTagAsync(
                    It.IsAny<string>(),
                    It.IsAny<IDatabaseTransaction>(),
                    CancellationToken.None),
                Times.Never);

            repoMock.Verify();
            sagaService.Verify();

            sagaService.Verify(
                s => s.ExecuteBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}
