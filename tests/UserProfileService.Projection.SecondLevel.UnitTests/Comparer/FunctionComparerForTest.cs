using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Comparer;

// only way to combine Moq with Fluent.Assertions => Expression tree of Moq method
// cannot contain Should().AssignableTo() calls
public class FunctionComparerForTest : IEqualityComparer<Function>
{
    public bool Equals(Function x, Function y)
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

    public int GetHashCode(Function obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Id);
        hashCode.Add(obj.OrganizationId);
        hashCode.Add(obj.Organization);
        hashCode.Add(obj.Role);
        hashCode.Add(obj.RoleId);
        hashCode.Add(obj.CreatedAt);
        hashCode.Add(obj.UpdatedAt);
        hashCode.Add(obj.SynchronizedAt);
        hashCode.Add(obj.Source);
        hashCode.Add(obj.ExternalIds);

        return hashCode.ToHashCode();
    }
}