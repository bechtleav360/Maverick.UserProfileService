using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Projection.FirstLevel.Tests.Comparer
{
    internal class AssignmentsComparer : IEqualityComparer<Assignment>
    {
        public bool Equals(Assignment x, Assignment y)
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

            y.Conditions.Should().AllBeEquivalentTo(x.Conditions);

            y.Should().BeEquivalentTo(x);

            return true;
        }

        public int GetHashCode(Assignment obj)
        {
            return HashCode.Combine((int)obj.TargetType, obj.TargetId, obj.ProfileId, obj.Conditions);
        }
    }
}
