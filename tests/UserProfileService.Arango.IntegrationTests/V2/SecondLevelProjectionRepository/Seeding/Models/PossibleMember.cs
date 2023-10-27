using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class PossibleMember
    {
        public string ChildId { get; }
        public ProfileKind ChildKind { get; }
        public List<ExtendedRangeCondition> Conditions { get; }
        public string ParentId { get; }
        public ContainerType ParentType { get; }

        public PossibleMember(IEnumerable<SingleAssignment> assignments)
        {
            List<SingleAssignment> temp = assignments.ToList();

            if (temp.Count == 0)
            {
                throw new Exception();
            }

            ChildId = temp[0].ChildProfileId;
            ParentId = temp[0].ParentId;
            ParentType = temp[0].ParentType;
            ChildKind = temp[0].ChildProfileKind;

            Conditions = temp.Select(a => new ExtendedRangeCondition(a.Start, a.End, a.SimulateConditionTriggered))
                .ToList();
        }
    }
}
