using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Moq;

namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    /// <summary>
    ///     Inspired by <see cref="It" />. Combines Moq and FluentAssertions.
    /// </summary>
    public class ItShould
    {
        public static T BeEquivalentTo<T>(
            T referenceObject,
            string because)
        {
            return It.Is(referenceObject, new FluentAssertionMeetsMoqComparer<T>(because));
        }

        public static T BeEquivalentTo<T>(
            T referenceObject,
            string because,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> configuration)
        {
            return It.Is(referenceObject, new FluentAssertionMeetsMoqComparer<T>(configuration, because));
        }

        public static T BeEquivalentTo<T>(
            T referenceObject)
        {
            return It.Is(referenceObject, new FluentAssertionMeetsMoqComparer<T>());
        }

        public static T BeEquivalentTo<T>(
            T referenceObject,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> configuration)
        {
            return It.Is(referenceObject, new FluentAssertionMeetsMoqComparer<T>(configuration));
        }

        private class FluentAssertionMeetsMoqComparer<T> : IEqualityComparer<T>
        {
            private readonly string _because;
            private readonly Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> _configuration;

            public FluentAssertionMeetsMoqComparer(string because = "")
            {
                _because = because;
            }

            public FluentAssertionMeetsMoqComparer(
                Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> configuration,
                string because = "")
            {
                _configuration = configuration;
                _because = because;
            }

            public bool Equals(T x, T y)
            {
                if (_configuration != null)
                {
                    x.Should().BeEquivalentTo(y, _configuration, _because);
                }
                else
                {
                    x.Should().BeEquivalentTo(y, _because);
                }

                return true;
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
