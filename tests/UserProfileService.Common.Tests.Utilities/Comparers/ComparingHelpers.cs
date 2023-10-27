using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    /// <summary>
    ///     Contains methods that will support comparing objects.
    /// </summary>
    public static class ComparingHelpers
    {
        /// <summary>
        ///     Checks if the first value is "almost" equal to the second value. The difference should be less than a tolerance.
        /// </summary>
        public static bool AboutEqual(
            double firstValue,
            double secondValue,
            double tolerance = double.Epsilon)
        {
            return Math.Abs(firstValue - secondValue) < tolerance;
        }

        public static bool CompareExternalIds(
            this IList<ExternalIdentifier> first,
            IList<ExternalIdentifier> second,
            ITestOutputHelper output = null)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null)
            {
                output?.WriteLine("External ids of first profile is null, but not the other!");

                return false;
            }

            if (second == null)
            {
                output?.WriteLine("External ids of second profile is null, but not the other!");

                return true;
            }

            return first.SequenceEqual(second, new ExternalIdComparer());
        }

        public static bool CompareStringLists(
            IList<string> x,
            IList<string> y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            return ReferenceEquals(x, y) || x.SequenceEqual(y);
        }

        public static bool CompareTagLists(
            IList<Tag> x,
            IList<Tag> y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            return ReferenceEquals(x, y) || x.SequenceEqual(y, new TestingEqualityComparerForTags());
        }

        public static bool CompareRangeConditions(
            IList<RangeCondition> x,
            IList<RangeCondition> y)
        {
            return (x == null && y == null)
                || (x != null && y != null && (ReferenceEquals(x, y) || x.SequenceEqual(y)));
        }

        public static bool CompareLinkedProfiles(
            IList<Member> x,
            IList<Member> y)
        {
            return (x == null && y == null)
                || (x != null
                    && y != null
                    && (ReferenceEquals(x, y) || x.SequenceEqual(y, new TestingEqualityComparerForMembers())));
        }
    }
}
