using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;
using ApiModels = Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding
{
    internal static class ConversionUtils
    {
        public static SingleAssignment ToAssignment(
            this AssignedToAttribute assigned,
            string childId,
            ProfileKind childKind)
        {
            return assigned != null
                ? new SingleAssignment
                {
                    ChildProfileId = childId,
                    ParentId = assigned.ParentId,
                    ParentType = assigned.ParentType,
                    Start = assigned.Start,
                    End = assigned.End,
                    ChildProfileKind = childKind,
                    SimulateConditionTriggered = assigned.SimulateConditionTriggered
                }
                : null;
        }

        public static IList<SingleAssignment> ToAssignments(
            this IEnumerable<AssignedToAttribute> assigned,
            string childId,
            ProfileKind childKind)
        {
            return assigned?
                    .Select(o => o.ToAssignment(childId, childKind))
                    .Where(o => o != null)
                    .ToList()
                ?? new List<SingleAssignment>();
        }

        public static IList<TagAssignmentId> ToTagList(this IEnumerable<HasTagAttribute> tagged)
        {
            return tagged?
                    .Select(t => new TagAssignmentId(t?.TagId, t?.IsInheritable ?? false))
                    .Where(t => !string.IsNullOrWhiteSpace(t.TagId))
                    .ToList()
                ?? new List<TagAssignmentId>();
        }

        public static AggregateModels.RangeCondition ToAggregateRangeCondition(
            this ExtendedRangeCondition rangeCondition)
        {
            return new AggregateModels.RangeCondition
            {
                Start = rangeCondition.Start,
                End = rangeCondition.End
            };
        }

        public static ApiModels.RangeCondition ToApiRangeCondition(
            this ExtendedRangeCondition rangeCondition)
        {
            return new ApiModels.RangeCondition(rangeCondition.Start, rangeCondition.End);
        }

        public static IEnumerable<ApiModels.RangeCondition> ToApiRangeConditions(
            this IEnumerable<ExtendedRangeCondition> rangeConditions)
        {
            return rangeConditions.Select(ToApiRangeCondition);
        }

        public static ApiModels.Member ToApiMember(
            this ExtendedMember member,
            bool ignoreSimulationFlag = true)
        {
            return member != null
                ? new ApiModels.Member
                {
                    DisplayName = member.Original.DisplayName,
                    Id = member.Original.Id,
                    Kind = member.Original.Kind,
                    Name = member.Original.Name,
                    ExternalIds = member.Original.ExternalIds.ToList(),
                    Conditions = member.RangeConditions?
                            .Where(c => ignoreSimulationFlag || !c.OnlyValidForSimulation)
                            .Select(c => c.ToApiRangeCondition())
                            .ToList()
                        ?? new List<ApiModels.RangeCondition>()
                }
                : null;
        }

        public static IList<ApiModels.Member> ToApiMembers(
            this IEnumerable<ExtendedMember> members,
            bool ignoreSimulationFlag = true)
        {
            return members
                .Select(m => m.ToApiMember(ignoreSimulationFlag))
                .Where(m => m?.Conditions != null && m.Conditions.Any())
                .ToList();
        }
    }
}
