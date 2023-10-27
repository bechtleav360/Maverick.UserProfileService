using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Comparer
{
    internal class FirstLevelProfileComparer : IEqualityComparer<IFirstLevelProjectionProfile>
    {
        public bool Equals(IFirstLevelProjectionProfile x, IFirstLevelProjectionProfile y)
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
                        opt.Excluding(u => u.UpdatedAt).Excluding(u => u.SynchronizedAt));

            return true;
        }

        public int GetHashCode(IFirstLevelProjectionProfile obj)
        {
            var hashCode = new HashCode();

            if (obj is FirstLevelProjectionUser u)
            {
                hashCode.Add(u.Email);
                hashCode.Add(u.FirstName);
                hashCode.Add(u.LastName);
                hashCode.Add(u.UserName);
                hashCode.Add(u.UserStatus);
            }

            if (obj is FirstLevelProjectionGroup g)
            {
                hashCode.Add(g.ContainerType);
                hashCode.Add(g.IsMarkedForDeletion);
            }

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
