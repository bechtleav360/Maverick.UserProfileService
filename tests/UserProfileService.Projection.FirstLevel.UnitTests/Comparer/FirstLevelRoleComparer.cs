using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Comparer
{
    internal class FirstLevelRoleComparer : IEqualityComparer<FirstLevelProjectionRole>
    {
        public bool Equals(FirstLevelProjectionRole x, FirstLevelProjectionRole y)
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
                    opt =>
                        opt.Excluding(u => u.SynchronizedAt));

            return true;
        }

        public int GetHashCode(FirstLevelProjectionRole obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.DeniedPermissions);
            hashCode.Add(obj.Description);
            hashCode.Add(obj.ExternalIds);
            hashCode.Add(obj.Id);
            hashCode.Add(obj.IsSystem);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Permissions);
            hashCode.Add(obj.Source);
            hashCode.Add(obj.SynchronizedAt);
            hashCode.Add(obj.UpdatedAt);

            return hashCode.ToHashCode();
        }
    }
}
