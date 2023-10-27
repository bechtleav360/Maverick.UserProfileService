using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Bogus;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class FluentAssertionsExtensions
    {
        private static void CheckList(IAssertionContext<IEnumerable> context)
        {
            if (context.Expectation == null)
            {
                context.Subject.Should().NotBeNull();
            }
            else
            {
                context.Subject.Should().BeEquivalentTo(context.Expectation);
            }
        }

        private static void CheckList<TListElement>(IAssertionContext<IList<TListElement>> context)
        {
            if (context.Expectation == null
                || context.Expectation.Count == 0)
            {
                context.Subject.Should().BeNullOrEmpty();
            }
            else
            {
                context.Subject.Should().BeEquivalentTo(context.Expectation);
            }
        }

        public static EquivalencyAssertionOptions<TCompareObject> TreatEmptyListsAndNullTheSame<TCompareObject>(
            this EquivalencyAssertionOptions<TCompareObject> options)
        {
            options.Using<IEnumerable>(CheckList)
                .When(oInfo => typeof(IEnumerable).IsAssignableFrom(oInfo.RuntimeType));

            return options;
        }

        public static EquivalencyAssertionOptions<TCompareObject> TreatEmptyListsAndNullTheSame<TCompareObject, TListElement>(
            this EquivalencyAssertionOptions<TCompareObject> options,
            Expression<Func<TCompareObject, IList<TListElement>>> _)
        {
            options.Using<IList<TListElement>>(CheckList)
                .When(oInfo => typeof(IList<>).MakeGenericType(typeof(TListElement)).IsAssignableFrom(oInfo.RuntimeType));

            return options;
        }
    }
}
