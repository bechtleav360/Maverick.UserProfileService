using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Comparer;

// only way to combine Moq with Fluent.Assertions => Expression tree of Moq method
// cannot contain Should().AssignableTo() calls
public class TagComparerForTest : IEqualityComparer<Tag>
{
    public bool Equals(Tag x, Tag y)
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

    public int GetHashCode(Tag obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Id);
        hashCode.Add(obj.Name);
        hashCode.Add((int)obj.Type);

        return hashCode.ToHashCode();
    }
}