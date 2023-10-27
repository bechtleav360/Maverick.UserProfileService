using System;
using Newtonsoft.Json;
using ApiModels = Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class ExtendedRangeCondition : ApiModels.RangeCondition
    {
        [JsonIgnore]
        internal bool OnlyValidForSimulation { get; set; }

        public ExtendedRangeCondition()
        {
        }

        public ExtendedRangeCondition(
            DateTime? start,
            DateTime? end,
            bool onlyValidForSimulation) : base(start, end)
        {
            OnlyValidForSimulation = onlyValidForSimulation;
        }
    }
}
