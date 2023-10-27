using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Tests.Mocks
{
    internal class MockedFirstLevelContainer : IFirstLevelProjectionContainer
    {
        public ContainerType ContainerType => ContainerType.NotSpecified;
        public string Id { get; set; }
    }
}
