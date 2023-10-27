using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class ExternalIdComparer : IEqualityComparer<ExternalIdentifier>
    {
        public bool Equals(ExternalIdentifier x, ExternalIdentifier y)
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

            return x.Id == y.Id
                && x.Source == y.Source;
        }

        public int GetHashCode(ExternalIdentifier obj)
        {
            return HashCode.Combine(obj.Id, obj.Source);
        }
    }
}
