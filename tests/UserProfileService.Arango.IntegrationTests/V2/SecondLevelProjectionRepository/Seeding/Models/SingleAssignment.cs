using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    public class SingleAssignment
    {
        public string ParentId { get; set; }
        public ContainerType ParentType { get; set; }
        public string ChildProfileId { get; set; }
        public ProfileKind ChildProfileKind { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public bool SimulateConditionTriggered { get; set; }
    }
}
