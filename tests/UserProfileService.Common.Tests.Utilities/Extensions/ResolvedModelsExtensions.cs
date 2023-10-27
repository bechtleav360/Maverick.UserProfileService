using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class ResolvedModelsExtensions
    {
        /// <summary>
        ///     Sets the range condition list in a member to a default value, if not set or empty, and returns it again.
        /// </summary>
        public static Member NormalizeRangeConditions(this Member source)
        {
            if (source == null
                || (source.Conditions != null
                    && source.Conditions.Count > 0))
            {
                return source;
            }

            source.Conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            return source;
        }

        public static Member AddRangeCondition(this Member source, List<RangeCondition> range)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            source.Conditions = source.Conditions == null ? range : source.Conditions.Concat(range).ToList();

            return source;
        }
    }
}
