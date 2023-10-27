using System;
using System.Collections.Generic;
using System.Linq;
using ApiModels = Maverick.UserProfileService.Models.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class RangeConditionExtensions
    {
        public static bool IsActive(
            this ApiModels.RangeCondition rangeCondition,
            DateTime? referenceDate = null)
        {
            if (rangeCondition == null)
            {
                return false;
            }

            DateTime explicitStart =
                rangeCondition.Start?.ToUniversalTime() ?? DateTime.MinValue;

            DateTime explicitEnd =
                rangeCondition.End?.ToUniversalTime() ?? DateTime.MaxValue;

            DateTime reference = referenceDate?.ToUniversalTime() ?? DateTime.UtcNow;

            return explicitStart <= reference && explicitEnd >= reference;
        }

        public static bool AnyActive(
            this IEnumerable<ApiModels.RangeCondition> rangeConditions,
            DateTime? referenceDate = null)
        {
            return rangeConditions?.Any(rc => rc.IsActive(referenceDate)) == true;
        }

        public static bool IsActive(
            this AggregateModels.RangeCondition rangeCondition,
            DateTime? referenceDate = null)
        {
            if (rangeCondition == null)
            {
                return false;
            }

            DateTime explicitStart =
                rangeCondition.Start?.ToUniversalTime() ?? DateTime.MinValue;

            DateTime explicitEnd =
                rangeCondition.End?.ToUniversalTime() ?? DateTime.MaxValue;

            DateTime reference = referenceDate?.ToUniversalTime() ?? DateTime.UtcNow;

            return explicitStart <= reference && explicitEnd >= reference;
        }

        public static bool AnyActive(
            this IEnumerable<AggregateModels.RangeCondition> rangeConditions,
            DateTime? referenceDate = null)
        {
            return rangeConditions?.Any(rc => rc.IsActive(referenceDate)) == true;
        }
    }
}
