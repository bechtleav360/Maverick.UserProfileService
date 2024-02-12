using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Comparer
{
    internal class FirstLevelGroupComparer : IEqualityComparer<FirstLevelProjectionGroup>
    {
        public bool Equals(FirstLevelProjectionGroup x, FirstLevelProjectionGroup y)
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

            x.Should()
                .BeEquivalentTo(
                    y,
                    opt => opt.Excluding(g => g.SynchronizedAt).Excluding(g => g.IsMarkedForDeletion));

            return true;
        }

        public int GetHashCode(FirstLevelProjectionGroup obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.DisplayName);
            hashCode.Add(obj.ExternalIds);
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Source);
            hashCode.Add(obj.SynchronizedAt);
            hashCode.Add(obj.UpdatedAt);
            hashCode.Add(obj.IsSystem);
            hashCode.Add(obj.Weight);
            hashCode.Add(obj.IsMarkedForDeletion);

            return hashCode.ToHashCode();
        }
    }
}
