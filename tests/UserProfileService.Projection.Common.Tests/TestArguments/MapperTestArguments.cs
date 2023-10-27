using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.Utilities;

namespace UserProfileService.Projection.Common.Tests.TestArguments
{
    public static class MapperTestArguments
    {
        public static IEnumerable<object[]> GetCreateEventTestArguments()
        {
            yield return TestArgumentHelpers.GetArgumentArray(
                ResolvedEventFakers.NewFunctionCreated,
                typeof(FunctionBasic));

            yield return TestArgumentHelpers.GetArgumentArray(
                ResolvedEventFakers.NewRoleCreated,
                typeof(RoleBasic));

            yield return TestArgumentHelpers.GetArgumentArray(
                ResolvedEventFakers.NewGroupCreated,
                typeof(GroupBasic));

            yield return TestArgumentHelpers.GetArgumentArray(
                ResolvedEventFakers.NewUserCreated,
                typeof(UserBasic));

            yield return TestArgumentHelpers.GetArgumentArray(
                ResolvedEventFakers.NewOrganizationCreated,
                typeof(OrganizationBasic));
        }
    }
}
