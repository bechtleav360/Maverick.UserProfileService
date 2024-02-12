using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Comparer;

// only way to combine Moq with Fluent.Assertions => Expression tree of Moq method
// cannot contain Should().AssignableTo() calls
public class RoleComparerForTest : IEqualityComparer<SecondLevelProjectionRole>
{
    public bool Equals(SecondLevelProjectionRole x, SecondLevelProjectionRole y)
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

        // will throw an exception, if not true => fine enough in this context
        x.Should()
            .BeEquivalentTo(y);

        return true;
    }

    public int GetHashCode(SecondLevelProjectionRole obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Id);
        hashCode.Add(obj.Name);
        hashCode.Add(obj.CreatedAt);
        hashCode.Add(obj.UpdatedAt);
        hashCode.Add(obj.SynchronizedAt);
        hashCode.Add(obj.Source);
        hashCode.Add(obj.ExternalIds);

        return hashCode.ToHashCode();
    }
}