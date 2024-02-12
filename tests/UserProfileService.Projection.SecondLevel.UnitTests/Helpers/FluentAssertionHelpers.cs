using System;
using System.Linq.Expressions;
using FluentAssertions.Equivalency;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

internal static class FluentAssertionHelpers
{
    internal static EquivalencyAssertionOptions<TEntity> ExcludingMany<TEntity>(
        this EquivalencyAssertionOptions<TEntity> options,
        Expression<Func<TEntity, object>>[] excludeOptions)
    {
        if (excludeOptions == null || excludeOptions.Length == 0)
        {
            return options;
        }

        foreach (Expression<Func<TEntity, object>> option in excludeOptions)
        {
            if (option == null)
            {
                continue;
            }

            options.Excluding(option);
        }

        return options;
    }

    internal static EquivalencyAssertionOptions<TEntity> ExcludingMemberInfo<TEntity>(
        this EquivalencyAssertionOptions<TEntity> options,
        Expression<Func<IMemberInfo, bool>> excludeOptions)
    {
        if (excludeOptions == null)
        {
            return options;
        }

        options.Excluding(excludeOptions);

        return options;
    }
}