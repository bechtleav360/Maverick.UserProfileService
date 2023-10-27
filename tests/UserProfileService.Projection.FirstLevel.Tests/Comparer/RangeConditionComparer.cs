using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;

namespace UserProfileService.Projection.FirstLevel.Tests.Comparer
{
    internal class RangeConditionComparer : IEqualityComparer<RangeCondition>
    {
        public bool Equals(RangeCondition x, RangeCondition y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            x.Should().BeEquivalentTo(y);

            return true;
        }

        public int GetHashCode(RangeCondition obj)
        {
            return HashCode.Combine(obj.Start, obj.End);
        }
    }
}
