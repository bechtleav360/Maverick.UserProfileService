using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class CalculatedTagIdEqualityComparer : IEqualityComparer<CalculatedTag>
    {
        public bool Equals(CalculatedTag x, CalculatedTag y)
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

            return x.Id == y.Id;
        }

        public int GetHashCode(CalculatedTag obj)
        {
            return obj.Id != null ? obj.Id.GetHashCode() : 0;
        }
    }
}
