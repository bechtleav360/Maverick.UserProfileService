using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForMembers : IEqualityComparer<Member>
    {
        public bool Equals(
            Member x,
            Member y)
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

            return x.Id == y.Id
                && x.Name == y.Name
                && x.DisplayName == y.DisplayName
                && x.Kind == y.Kind
                && ComparingHelpers.CompareRangeConditions(x.Conditions, y.Conditions);
        }

        public int GetHashCode(Member obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, obj.DisplayName, (int)obj.Kind);
        }
    }
}
