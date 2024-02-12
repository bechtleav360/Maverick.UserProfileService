using System;
using System.Linq;
using Moq;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class FunctionTagsRemovedEventHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly FirstLevelProjectionFunction _function;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;
        private readonly FunctionTagsRemovedEvent _tagsRemovedEvent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionDeletedEventHandlerTest" /> class.
        /// </summary>
        public FunctionTagsRemovedEventHandlerTest()
        {
            _function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstances().Single();

            _tagsRemovedEvent = new FunctionTagsRemovedEvent(
                DateTime.Now,
                new TagsRemovedPayload
                {
                    ResourceId = _function.Id,
                    IsSynchronized = false,
                    Tags = MockDataGenerator.GenerateTagAssignments(NumberTagAssignments, true)
                        .Select(x => x.TagId)
                        .ToArray()
                });

            _mockSagaService = MockProvider.GetDefaultMock<ISagaService>();
            _mockCreator = MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();
        }
    }
}
