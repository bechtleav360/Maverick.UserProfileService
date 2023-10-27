using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.Tests.Utilities.Comparers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    /// <summary>
    ///     Contains extension methods regarding strings with ids, id objects, etc.
    /// </summary>
    public static class IdExtensions
    {
        public static List<ExternalIdentifier> CreateSimpleExternalIdentifiers(this string identifier)
        {
            return string.IsNullOrWhiteSpace(identifier)
                ? new List<ExternalIdentifier>()
                : new List<ExternalIdentifier>
                {
                    new ExternalIdentifier(identifier, "test")
                };
        }

        public static void AssertExternalIds<TProfile>(
            this TProfile first,
            TProfile second,
            ITestOutputHelper output = null)
            where TProfile : IProfile
        {
            if (first == null || second == null)
            {
                output?.WriteLine("At least one profile is null!");

                return;
            }

            if (first.ExternalIds == null && second.ExternalIds == null)
            {
                return;
            }

            if (first.ExternalIds == null)
            {
                throw new XunitException("External ids of first profile is null, but not the other!");
            }

            if (second.ExternalIds == null)
            {
                throw new XunitException("External ids of second profile is null, but not the other!");
            }

            Assert.Equal(first.ExternalIds, second.ExternalIds, new ExternalIdComparer());
        }
    }
}
