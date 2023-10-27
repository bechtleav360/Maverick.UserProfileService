using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class FunctionTagsAddedEventHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly FirstLevelProjectionFunction _function;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;
        private readonly FunctionTagsAddedEvent _tagsAddedEvent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionDeletedEventHandlerTest" /> class.
        /// </summary>
        public FunctionTagsAddedEventHandlerTest()
        {
            _function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances().Single();

            _tagsAddedEvent = new FunctionTagsAddedEvent(
                DateTime.Now,
                new TagsSetPayload
                {
                    Id = _function.Id,
                    IsSynchronized = false,
                    Tags = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true).ToArray()
                });

            _mockSagaService = MockProvider.GetDefaultMock<ISagaService>();
            _mockCreator = MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();
        }

        [Fact]
        public Task Handler_should_work()
        {
            //arrange
            Mock<IDatabaseTransaction> transaction = MockProvider.GetDefaultMock<IDatabaseTransaction>();

            Mock<IFirstLevelProjectionRepository> repoMock =
                MockProvider.GetDefaultMock<IFirstLevelProjectionRepository>();

            Mock<ISagaService> sagaService = MockProvider.GetDefaultMock<ISagaService>();
            Mock<IStreamNameResolver> streamResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();

            return Task.CompletedTask;
        }
    }
}
