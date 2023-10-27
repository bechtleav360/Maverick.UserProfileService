using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class AssignedToAttribute : Attribute
    {
        /// <summary>
        ///     If this flag is false (default value), the assignment has already been added to all collections, including
        ///     calculated ones like profilesQuery.<br />
        ///     If it has been set, this assignment will be ignored for calculated collection data (as in profile query
        ///     collections) to simulate AssignmentConditionTriggered.
        /// </summary>
        public bool SimulateConditionTriggered { get; }

        public ContainerType ParentType { get; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public string ParentId { get; }

        public AssignedToAttribute(
            string parentId,
            ContainerType parentType)
        {
            ParentId = parentId;
            ParentType = parentType;
        }

        public AssignedToAttribute(
            string parentId,
            ContainerType parentType,
            int validInDays,
            int validTillDays,
            bool simulateConditionTriggered) : this(parentId, parentType, validInDays, validTillDays)
        {
            SimulateConditionTriggered = simulateConditionTriggered;
        }

        public AssignedToAttribute(
            string parentId,
            ContainerType parentType,
            int validInDays,
            int validTillDays)
        {
            ParentId = parentId;
            ParentType = parentType;
            Start = DateTime.Today.AddDays(validInDays).ToUniversalTime();
            End = DateTime.Today.AddDays(validTillDays).ToUniversalTime();
        }
    }
}
