using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace UserProfileService.Sync.Projection.UnitTests.Utilities
{
    public class ObjectComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> _comparisonExpression;

        public bool Equals(T x, T y)
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

            if (_comparisonExpression == null)
            {
                // will throw an exception, if not true => fine enough in this context
                x.Should()
                 .BeEquivalentTo(y);
            }
            else
            {
                // will throw an exception, if not true => fine enough in this context
                x.Should()
                 .BeEquivalentTo(y, _comparisonExpression);
            }

            return true;
        }

        public int GetHashCode(T obj)
        {
            var hashCode = new HashCode();

            return hashCode.ToHashCode();
        }

        public ObjectComparer(Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> comparisonExpression)
        {
            _comparisonExpression = comparisonExpression;
        }

        public ObjectComparer()
        {
        }
    }
}
