using Moq;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.V2
{
    public class RolePropertiesChangedEventHandlerTest
    {
        private readonly Mock<IFirstLevelEventTupleCreator> _mockCreator;
        private readonly Mock<ISagaService> _mockSagaService;
        private readonly RolePropertiesChangedEvent _propertiesChangedEvent;
        private readonly FirstLevelProjectionRole _role;

        public RolePropertiesChangedEventHandlerTest(Mock<IFirstLevelEventTupleCreator> mockCreator)
        {
        }
    }
}
