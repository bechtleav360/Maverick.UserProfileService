using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForTags : IEqualityComparer<Tag>
    {
        public bool Equals(
            Tag x,
            Tag y)
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

            return x.Name == y.Name && x.Id == y.Id;
        }

        public int GetHashCode(Tag obj)
        {
            return HashCode.Combine(obj.Name);
        }
    }
}
