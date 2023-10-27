using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Comparer
{
    internal class FirstLevelUserComparer : IEqualityComparer<FirstLevelProjectionUser>
    {
        public bool Equals(FirstLevelProjectionUser x, FirstLevelProjectionUser y)
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

            x.Should()
                .BeEquivalentTo(
                    y,
                    opt =>
                        opt.Excluding(u => u.UserStatus).Excluding(u => u.SynchronizedAt));

            return true;
        }

        public int GetHashCode(FirstLevelProjectionUser obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Email);
            hashCode.Add(obj.FirstName);
            hashCode.Add(obj.LastName);
            hashCode.Add(obj.UserName);
            hashCode.Add(obj.UserStatus);
            hashCode.Add(obj.Source);
            hashCode.Add(obj.CreatedAt);
            hashCode.Add(obj.DisplayName);
            hashCode.Add(obj.ExternalIds);
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.SynchronizedAt);
            hashCode.Add(obj.UpdatedAt);

            return hashCode.ToHashCode();
        }
    }
}
