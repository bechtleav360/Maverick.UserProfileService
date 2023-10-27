using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.Tests.Comparer;

public class RangeConditionComparerForTest : IEqualityComparer<RangeCondition>
{
    public bool Equals(RangeCondition x, RangeCondition y)
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

        try
        {
            x.Should().BeEquivalentTo(y);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public int GetHashCode(RangeCondition obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.End);
        hashCode.Add(obj.Start);

        return hashCode.ToHashCode();
    }
}