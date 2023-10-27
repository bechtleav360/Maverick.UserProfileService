using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    [TestData(true, nameof(FunctionsTestData))]
    public class FunctionsTestData
    {
        public static class UpdateFunction
        {
            [Function("Update function test")]
            public const string FunctionId = "func_ea59c62f-2633-4573-b198-e21595d6b1e1";
        }

        public static class AddMember
        {
            [Function("Add member to function test")]
            public const string FunctionId = "func_64accd6c-fa50-4bc4-bdb6-2f04fa405899";

            [Group("added to function (addMemberFuncTest)")]
            [AssignedTo(FunctionId, ContainerType.Function)]
            public const string ExistingGroupId = "grp_afd44b6d-d723-4998-bebe-316623a94b96";

            [Group("will be added to function (addMemberFuncTest)")]
            public const string NewGroupId = "grp_eb3f7c5c-3140-42c2-83fc-9bc12b6ea7c7";
        }

        public static class AddMemberAddTwice
        {
            [Function("Add member twice to function test")]
            public const string FunctionId = "func_88093A44-0175-4469-AE64-327CF62BFF53";

            [Group("added to function (AddMemberAddTwice)")]
            [AssignedTo(FunctionId, ContainerType.Function)]
            public const string ExistingGroupId = "grp_4A62F126-3B82-4B38-B0D1-5D0C2591F277";

            [Group("will be added to function AddMemberTwice")]
            public const string NewGroupId = "grp_A466E36F-21C8-47D7-B897-A51A4B46A736";
        }

        public static class AddMemberRangeConditions
        {
            [Function("Add member with range conditions")]
            public const string FunctionId = "func_B00B9529-CD2B-4332-A71E-7D7082E0BF39";

            [Group("added to function already (AddMemberRangeConditions)")]
            [AssignedTo(FunctionId, ContainerType.Function)]
            public const string ExistingGroupId = "grp_8F5B3675-6BFF-4953-8E34-AB4137A0E134";

            [Group("will be added to function twice with sevecal range conditions")]
            public const string NewGroupId = "grp_B740FD19-25FA-4A5A-ACE6-0873B4CCA4B9";
        }
    }
}
