using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForCalculatedTags : IEqualityComparer<CalculatedTag>
    {
        public bool Equals(
            CalculatedTag x,
            CalculatedTag y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Name == y.Name && x.IsInherited == y.IsInherited;
        }

        public int GetHashCode(CalculatedTag obj)
        {
            return HashCode.Combine(obj.Name, obj.IsInherited);
        }
    }
}
