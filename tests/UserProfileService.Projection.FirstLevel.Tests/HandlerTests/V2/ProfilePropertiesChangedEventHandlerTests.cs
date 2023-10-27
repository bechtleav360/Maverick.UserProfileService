using System.Collections.Generic;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.HandlerTests.V2
{
    public class ProfilePropertiesChangedEventHandlerTests
    {
        private readonly ProfilePropertiesChangedEvent _propertiesChangedEventGroup;
        private readonly FirstLevelProjectionGroup _groupChanged;


        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionPropertiesChangedEventHandlerTest" /> class.
        /// </summary>
        public ProfilePropertiesChangedEventHandlerTests()
        {
            _groupChanged = MockDataGenerator.GenerateFirstLevelProjectionGroupWithId("_GroupChanged");

            _propertiesChangedEventGroup =
                MockedSagaWorkerEventsBuilder.CreatePropertiesChangedEvent(
                    _groupChanged,
                    new Dictionary<string, object> { { "name", "ChangeNameOfGroup" } });
        }
    }
}
