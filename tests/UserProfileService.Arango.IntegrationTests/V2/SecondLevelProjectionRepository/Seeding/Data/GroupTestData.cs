using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    [TestData(true, nameof(GroupTestData))]
    public class GroupTestData
    {
        public static class AddSecurityAssignmentsInExistingSet
        {
            [Function]
            public const string NewFunctionId = "func-as-part-of-group-test-to-be-added-in-sec-assignments";

            [Function]
            public const string ExistingFunctionId = "func-as-part-of-group-test-already-part-of-group";

            [Group("group-whose-security-assignments-should-be-updated as part of group test")]
            [AssignedTo(ExistingFunctionId, ContainerType.Function)]
            public const string GroupId = "grp-bba12b07-c193-4431-8db3-f4b2dadbb8cf";
        }

        public static class AddMemberToExistingSet
        {
            [Group("AddGroupMemberToExisting - the parent group")]
            public const string ParentGroupId = "parent-grp-e64c04ee-b6dd-4782-87e7-59c61a755e94";

            [Group("AddGroupMemberToExisting - already child of parent group")]
            [AssignedTo(ParentGroupId, ContainerType.Group)]
            public const string ExistingChildGroupId = "grp-f8a97712-82f6-4e46-9f17-33f8b4b03ce4";

            [User("AddGroupMemberToExisting - will be a new member of parent group")]
            public const string NewChildUserId = "user-1284b004-6c24-4893-aeaf-2145c0779f52";
        }

        public static class AddMemberOfEntryToExistingSet
        {
            [Group("AddGroupMemberOfToExisting - will be a new member of newParent group")]
            [AssignedTo(OldParentGroupId, ContainerType.Group)]
            public const string ChildGroupId = "child-grp-cdf152ea-30d4-4a0a-b97e-7f89e40515c2";

            [Group("AddGroupMemberOfToExisting - the parent group")]
            public const string NewParentGroupId = "old-parent-grp-280c99e3-d7b9-493a-8013-947a4042b663";

            [Group("AddGroupMemberOfToExisting - already parent of child group")]
            public const string OldParentGroupId = "new-parent-grp-a09bee75-6b6d-40f6-a12e-b85e1014c135";
        }
    }
}
