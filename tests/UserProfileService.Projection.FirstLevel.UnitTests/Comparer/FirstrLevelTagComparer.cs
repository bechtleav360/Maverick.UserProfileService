using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Comparer
{
    internal class FirstLevelTagComparer : IEqualityComparer<FirstLevelProjectionTag>
    {
        public bool Equals(FirstLevelProjectionTag x, FirstLevelProjectionTag y)
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

        public int GetHashCode(FirstLevelProjectionTag obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, (int)obj.Type);
        }
    }
}
