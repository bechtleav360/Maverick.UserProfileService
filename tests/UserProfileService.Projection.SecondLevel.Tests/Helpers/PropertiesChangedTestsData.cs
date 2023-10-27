using System.Linq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Tests.Helpers;

internal class PropertiesChangedTestsData
{
    internal SecondLevelProjectionGroup GroupOfGroup { get; }
    internal SecondLevelProjectionGroup GroupOfReferenceUser { get; }
    internal SecondLevelProjectionFunction ReferenceFunction { get; }
    internal SecondLevelProjectionUser ReferenceUser { get; }

    public PropertiesChangedTestsData()
    {
        ReferenceUser = MockDataGenerator.GenerateSecondLevelProjectionUser(1, 1, 1).Single();
        ReferenceFunction = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();
        SecondLevelProjectionGroup sampleGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
        GroupOfReferenceUser = sampleGroup.Merge(ReferenceUser.MemberOf.First());

        GroupOfGroup =
            MockDataGenerator.GenerateSecondLevelProjectionGroup(1, 0, 0).Single();

        GroupOfGroup.MemberOf.Add(ReferenceUser.MemberOf.First());
    }
}