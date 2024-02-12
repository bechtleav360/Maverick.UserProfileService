using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Comparer;

// only way to combine Moq with Fluent.Assertions => Expression tree of Moq method
// cannot contain Should().AssignableTo() calls
internal class ProfileComparerForTest : IEqualityComparer<ISecondLevelProjectionProfile>
{
    private readonly Expression<Func<ISecondLevelProjectionProfile, object>>[] _excludedProperties;
    private readonly Expression<Func<IMemberInfo, bool>> _excludedPropertyInfo;

    private ProfileComparerForTest(
        Expression<Func<IMemberInfo, bool>> excludedPropertyInfo)
    {
        _excludedPropertyInfo = excludedPropertyInfo;
    }

    public ProfileComparerForTest(
        params Expression<Func<ISecondLevelProjectionProfile, object>>[] excludedProperties)
    {
        _excludedProperties = excludedProperties;
    }

    public static ProfileComparerForTest CreateWithExcludeMember(
        Expression<Func<IMemberInfo, bool>> excludedPropertyInfo)
    {
        return new ProfileComparerForTest(excludedPropertyInfo);
    }

    public bool Equals(ISecondLevelProjectionProfile x, ISecondLevelProjectionProfile y)
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
                options =>
                    options
                        .Excluding(
                            info => info.Path.Contains("Url")
                                || info.Path == nameof(ISecondLevelProjectionProfile.Paths))
                        .ExcludingMany(_excludedProperties)
                        .ExcludingMemberInfo(_excludedPropertyInfo));

        return true;
    }

    public int GetHashCode(ISecondLevelProjectionProfile obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Id);
        hashCode.Add(obj.Name);
        hashCode.Add(obj.DisplayName);
        hashCode.Add(obj.ExternalIds);
        hashCode.Add(obj.Source);
        hashCode.Add((int)obj.Kind);
        hashCode.Add(obj.CreatedAt);
        hashCode.Add(obj.UpdatedAt);
        hashCode.Add(obj.SynchronizedAt);

        return hashCode.ToHashCode();
    }
}