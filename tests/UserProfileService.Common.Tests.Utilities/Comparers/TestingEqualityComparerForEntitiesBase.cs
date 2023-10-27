using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UserProfileService.Common.Tests.Utilities.Extensions;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public abstract class TestingEqualityComparerForEntitiesBase<T> : IEqualityComparer<T> where T : class
    {
        protected ITestOutputHelper OutputHelper;

        protected TestingEqualityComparerForEntitiesBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual bool ShallCheckTypeEquality()
        {
            return true;
        }

        protected abstract string GetId(T input);

        protected abstract bool EqualsInternally(T x, T y);

        protected static bool IsTrue(params bool[] input)
        {
            return input.All(o => o);
        }

        protected bool AddOutput<TProp>(
            bool check,
            T firstEntity,
            T secondEntity,
            Expression<Func<T, TProp>> propertySelector)
        {
            if (check)
            {
                return true;
            }

            string propertyName = (propertySelector.Body as MemberExpression)?.Member.Name ?? "unknown";

            Func<T, TProp> func = propertySelector.Compile();
            TProp first = func.Invoke(firstEntity);
            TProp second = func.Invoke(secondEntity);

            OutputHelper.WriteLine(
                $"Property '{propertyName}' of both {typeof(T).Name} (id: '{GetId(firstEntity)}') not equal: '{first.ConvertToOutputStringDuringTest()}' <=> '{second.ConvertToOutputStringDuringTest()}'.");

            return false;
        }

        public virtual bool Equals(
            T x,
            T y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                OutputHelper.WriteLine(
                    $"{Environment.NewLine}{typeof(T).Name} objects are not equal! The first is null, but the other is not (id: {GetId(y)}).");

                return false;
            }

            if (y == null)
            {
                OutputHelper.WriteLine(
                    $"{Environment.NewLine}{typeof(T).Name} objects are not equal! The second is null, but the other is not ({GetId(x)}).");

                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ShallCheckTypeEquality() && x.GetType() != y.GetType())
            {
                OutputHelper.WriteLine(
                    $"{Environment.NewLine}{typeof(T).Name} objects are not of same type! {x.GetType().Name} ({GetId(x)}) != {y.GetType().Name} ({GetId(y)})");

                return false;
            }

            return EqualsInternally(x, y);
        }

        public abstract int GetHashCode(T obj);
    }
}
