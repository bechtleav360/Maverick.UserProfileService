using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Comparer
{
    public class ObjectIdentComparer : IEqualityComparer<ObjectIdent>
    {
        public bool Equals(ObjectIdent x, ObjectIdent y)
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
                && x.Type == y.Type;
        }

        public int GetHashCode(ObjectIdent obj)
        {
            return HashCode.Combine(obj.Id, (int)obj.Type);
        }
    }
}
