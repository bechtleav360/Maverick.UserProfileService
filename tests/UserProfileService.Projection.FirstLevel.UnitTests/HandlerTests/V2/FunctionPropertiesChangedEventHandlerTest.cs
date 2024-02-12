using Moq;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class FunctionPropertiesChangedEventHandlerTest
    {
        private const int NumberTagAssignments = 10;
        private readonly FirstLevelProjectionFunction _function;
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;
        private readonly FunctionPropertiesChangedEvent _propertiesChangedEvent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionPropertiesChangedEventHandlerTest" /> class.
        /// </summary>
        public FunctionPropertiesChangedEventHandlerTest()
        {
        }
    }
}
