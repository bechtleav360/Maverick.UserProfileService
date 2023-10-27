using System;
using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Tests.Comparer;

public class SecondLevelProjectionFunctionComparerForTest : IEqualityComparer<SecondLevelProjectionFunction>
{
    public bool Equals(SecondLevelProjectionFunction x, SecondLevelProjectionFunction y)
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
            .BeEquivalentTo(
                y,
                options => options
                    .Excluding(info => info.Path.Contains("Url")));

        return true;
    }

    public int GetHashCode(SecondLevelProjectionFunction obj)
    {
        var hashCode = new HashCode();

        return hashCode.ToHashCode();
    }
}