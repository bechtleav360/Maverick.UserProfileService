using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Common.Tests.Utilities.FluentApi;

namespace UserProfileService.Projection.Common.Tests.Helpers
{
    public static class ConverterTestArguments
    {
        // Arguments: ObjectType, Dictionary<string, object> as property bag
        public static IEnumerable<object[]> PropertiesChangedEventShouldWork =>
            TestArgumentBuilder
                .CreateArguments()
                // user test case
                .AddTestCaseParameters(
                    ObjectType.User,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "name", "Test user #1" },
                        { "ExternalIds", new[] { new ExternalIdentifier("S-1-156-891", "ActiveDirectory") } }
                    },
                    null)
                // group test case
                .AddTestCaseParameters(
                    ObjectType.Group,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { nameof(GroupBasic.ImageUrl), "https://my.image.io/cool-stuff" },
                        { nameof(GroupBasic.Kind), ProfileKind.User }, // makes no sense, but just for the test
                        { nameof(GroupBasic.CreatedAt), DateTime.UtcNow },
                        { nameof(GroupBasic.IsMarkedForDeletion), true },
                        { nameof(GroupBasic.Weight), 34.561 }
                    },
                    null)
                .AddTestCaseParameters(
                    ObjectType.Organization,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "IsSubOrgAniZatIon", true },
                        { nameof(OrganizationBasic.SynchronizedAt), null },
                        { "notMyProperty", "one-two-three" }
                    },
                    new[] { "notMyProperty" })
                .AddTestCaseParameters(
                    ObjectType.Function,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "NAME", "new Name" },
                        { nameof(FunctionBasic.OrganizationId), null }
                    },
                    null)
                .AddTestCaseParameters(
                    ObjectType.Role,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            nameof(RoleBasic.Permissions), new List<string>
                            {
                                "Write",
                                "Read",
                                "Management #1"
                            }
                        },
                        { nameof(RoleBasic.ExternalIds), new List<ExternalIdentifier>() }
                    },
                    null)
                .AddTestCaseParameters(
                    ObjectType.User,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                    null)
                .Build();

        public static IEnumerable<object[]> PropertiesChangedEventShouldFail =>
            TestArgumentBuilder.CreateArguments()
                .AddTestCaseParameters(
                    ObjectType.Profile,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "name", 1 }
                    },
                    typeof(NotSupportedException))
                .AddTestCaseParameters(
                    ObjectType.Unknown,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "name", 1 }
                    },
                    typeof(InvalidOperationException))
                .AddTestCaseParameters(
                    999,
                    new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "name", 1 }
                    },
                    typeof(ArgumentOutOfRangeException))
                .Build();
    }
}
