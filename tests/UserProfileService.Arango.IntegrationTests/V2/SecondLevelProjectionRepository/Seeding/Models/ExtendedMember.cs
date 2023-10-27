using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class ExtendedMember
    {
        internal Member Original { get; set; }
        internal List<ExtendedRangeCondition> RangeConditions { get; set; }

        public ExtendedMember(
            Member original)
        {
            Original = original;
        }
    }
}
