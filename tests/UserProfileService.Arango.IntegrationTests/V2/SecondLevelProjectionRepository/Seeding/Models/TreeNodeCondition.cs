using System.Collections.Generic;
using System.Linq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using ApiModels = Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class TreeNodeCondition
    {
        public IList<ExtendedRangeCondition> Conditions { get; }
        public TreeNode Node { get; }

        public TreeNodeCondition(
            IList<ExtendedRangeCondition> conditions,
            TreeNode node)
        {
            Conditions = conditions != null
                && conditions
                    .Any(c => c != null)
                    ? conditions.Where(c => c != null).ToList()
                    : new List<ExtendedRangeCondition>
                    {
                        new ExtendedRangeCondition()
                    };

            Node = node;
        }

        public TreeNodeCondition(
            IList<ApiModels.RangeCondition> conditions,
            TreeNode node)
        {
            Conditions = conditions != null
                && conditions
                    .Any(c => c != null)
                    ? conditions.Where(c => c != null)
                        .Select(c => new ExtendedRangeCondition(c.Start, c.End, false))
                        .ToList()
                    : new List<ExtendedRangeCondition>
                    {
                        new ExtendedRangeCondition()
                    };

            Node = node;
        }

        public bool IsRelationActive(bool ignoreSimulationFlag)
        {
            return Conditions
                .Where(c => ignoreSimulationFlag || !c.OnlyValidForSimulation)
                .AnyActive();
        }

        public override string ToString()
        {
            return
                $"{Node} - {Conditions.Count} condition(s) - at the moment {(Conditions.AnyActive() ? "active" : "inactive")}";
        }
    }
}
