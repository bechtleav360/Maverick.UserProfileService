using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class AssignedToAttribute : Attribute
    {
        public string TargetId { get; set; }
        public ContainerType TargetType { get; set; }
        public RangeCondition Conditions { get; set; }

        public AssignedToAttribute(string targetId, ContainerType type)
        {
            TargetId = targetId;
            TargetType = type;
            Conditions = new RangeCondition();
        }

        public AssignedToAttribute(string targetId, ContainerType type, DateTime start, DateTime end)
        {
            TargetId = targetId;
            TargetType = type;

            Conditions = new RangeCondition
            {
                Start = start,
                End = end
            };
        }

        public AssignedToAttribute(string targetId, ContainerType type, int startsInDays, int endsInDays)
        {
            TargetId = targetId;
            TargetType = type;

            Conditions = new RangeCondition
            {
                Start = DateTime.Today.AddDays(startsInDays).ToUniversalTime(),
                End = DateTime.Today.AddDays(endsInDays).ToUniversalTime()
            };
        }
    }
}
