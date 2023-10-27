using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    [SeedData]
    public static class ProfilesWriteTestData
    {
        #region Add Assignments

        public static class AssignProfileToGroupTemporarily
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "6078678E-A5E3-48FD-9DAB-58701B15EDC1";

            [Profile(ProfileKind.Group)]
            public const string GroupId = "0E289C2B-2CBE-4633-88FD-F6B185D65BC9";
        }

        public static class AssignUserToGroup
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "920a92cb-a3ca-497f-821f-c57d5cee40f3";

            [Profile(ProfileKind.Group)]
            public const string GroupId = "207ee891-f1ec-424b-868c-f58091776cfa";
        }

        public static class AssignGroupToGroup
        {
            [Profile(ProfileKind.User)]
            public const string MemberId = "5bd8ed95-dea4-4505-9db5-49176183c947";

            [Profile(ProfileKind.Group)]
            public const string ParentId = "aaa65d01-6975-4204-9944-4430df780436";
        }

        public static class AssignNotExistingProfileToGroup
        {
            [Profile(ProfileKind.Group)]
            public const string ParentId = "b9876a91-0b8a-4d95-85e2-44a57dd0f519";
        }

        public static class AssignUserToNotExistingProfile
        {
            [Profile(ProfileKind.User)]
            public const string ParentId = "a423b4a1-1b5b-4aad-800b-e2fc95720855";
        }

        public static class AssignGroupToNotExistingProfile
        {
            [Profile(ProfileKind.Group)]
            public const string ParentId = "cd0f0c75-905e-4baa-84ad-bf1b73d6d305";
        }

        public static class AssignOrganizationToNotExistingProfile
        {
            [Profile(ProfileKind.Organization)]
            public const string ParentId = "db1249a1-6827-414b-8f97-2e118ef9bd9a";
        }

        public static class AssignOrgUnitToOrgUnit
        {
            [Profile(ProfileKind.Organization)]
            public const string MemberId = "43d9aed1-a38a-4149-8fa8-14c2693de998";

            [Profile(ProfileKind.Organization)]
            public const string ParentId = "83f55f62-f451-4689-bb4f-057bfe2b87ba";
        }

        public static class AssignUserToRole
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "b19b4621-3735-464a-b7bb-34dbe54b916f";

            [Role(Name = "Test-Role")]
            public const string RoleId = "18048612-94c0-4b59-965f-f5775f027bc4";
        }

        public static class AssignGroupToRoleConditionally
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "ade60f6e-fb0b-453b-b541-78de1d1059ea";

            [Role(Name = "Test-Role")]
            public const string RoleId = "324b90e5-aa4c-4e61-a61e-029cdcea2c5b";
        }

        public static class AssignGroupToRole
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "30c2be7a-3601-4de2-a308-50db5b2bddec";

            [Role(Name = "Test-Role")]
            public const string RoleId = "324b90e5-aa4c-4e61-a61e-029cdcea2c5b";
        }

        public static class AssignUserToNotExistingRole
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "b3516f64-123e-48de-ae4f-f8b2820655cd";
        }

        public static class AssignGroupToNotExistingRole
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "cb770602-ca39-4bb7-a280-eab5768ab0e7";
        }

        public static class AssignUserToFunction
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "02bcd28a-7762-4b6f-a050-7bf7b7773e34";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "dd2f88e6-b783-450f-a6bd-345b9c5cf6c1";

            [Role(Name = "Test-Role")]
            public const string RoleId = "6784b86b-fd34-43de-a421-87ea3fec2dfa";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "2d7a3c60-a2d7-4429-ae13-e96fe6df25ee";
        }

        public static class AssignGroupToFunction
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "387075b2-6132-4da2-9c81-5a15e62cb86e";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "00a9e065-e3c6-4808-944c-5f49199756a8";

            [Role(Name = "Test-Role")]
            public const string RoleId = "a1886e2c-8d15-4132-a681-157661c5710f";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "1052757a-fa9d-458a-af9b-ae1baa834e35";
        }

        #endregion

        #region Remove Assignments

        public static class RemoveInfiniteUserToGroupAssignment
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(GroupId, ContainerType.Group)]
            public const string UserId = "ebc89ff7-cf33-465d-aa68-c2ab47632b6a";

            [Profile(ProfileKind.Group)]
            public const string GroupId = "b6081f86-7f08-4708-8f90-b7414c9590c4";
        }

        public static class RemoveInfiniteUserToRoleAssignment
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(RoleId, ContainerType.Role)]
            public const string UserId = "13171147-75be-4429-8ca5-d0e1ec2044dc";

            [Role]
            public const string RoleId = "bb753acc-2ec1-499d-8ed2-e5ad3a28cb8a";
        }

        public static class RemoveInfiniteUserToFunctionAssignment
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(FunctionId, ContainerType.Function)]
            public const string UserId = "e5ec9e23-1c29-4652-be47-74399356b50c";

            [Role]
            public const string RoleId = "685a1696-756f-46a0-ab9c-b63c3ab9a521";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "429e5d14-214c-4911-b0a8-da1e13b44842";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "2e8a3b6a-e681-4f84-9b91-57378d52d567";
        }

        public static class RemoveConditionalUserToGroupAssignment
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(GroupId, ContainerType.Group, -5, 5)]
            public const string UserId = "fc4799ba-c5ca-4e95-8646-1c30cbd429a9";

            [Profile(ProfileKind.Group)]
            public const string GroupId = "02897f78-97cb-48f2-ae1b-7d589c5eb4a0";
        }

        public static class RemoveOneConditionalUserToGroupAssignment
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(GroupId, ContainerType.Group, -10, 0)]
            [AssignedTo(GroupId, ContainerType.Group, 5, 10)]
            public const string UserId = "0899e17e-c5dd-424b-ab33-278b498bb96b";

            [Profile(ProfileKind.Group)]
            public const string GroupId = "508d4a2d-7b0a-4fab-985b-d7274ee1b64e";
        }

        #endregion

        #region ClientSettings

        public static class AddClientSettingsToUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "7a3ff718-07dd-4a79-8647-5db434d703c5";
        }

        public static class AddClientSettingsToGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "645f4c6e-709f-4238-aa0f-ce8438b96dd3";
        }

        public static class UpdateClientSettingsOfUser
        {
            [Profile(ProfileKind.User)]
            [HasClientSettings(Key, "{\"value\":1}")]
            public const string UserId = "ac186dbb-0047-4d64-b175-90528e59e723";

            public const string Key = "test";
        }

        public static class UpdateClientSettingsOfGroup
        {
            [Profile(ProfileKind.Group)]
            [HasClientSettings(Key, "{\"value\":1}")]
            public const string GroupId = "9290e16d-209f-4e27-b4e5-abb28cb90b57";

            public const string Key = "test";
        }

        public static class UnsetClientSettingsOfUser
        {
            [Profile(ProfileKind.User)]
            [HasClientSettings(Key, "{\"value\":1}")]
            public const string UserId = "261dfc4e-a919-4dbe-b23d-3c5ebe6ba15f";

            public const string Key = "unset-test";
        }

        public static class UnsetClientSettingsOfGroup
        {
            [Profile(ProfileKind.Group)]
            [HasClientSettings(Key, "{\"value\":1}")]
            public const string GroupId = "cd5f305b-6570-40db-9ba1-24ab60eb0bb1";

            public const string Key = "unset-test";
        }

        #endregion

        #region Deletion

        public static class DeleteUser
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(ParentGroupId, ContainerType.Group)]
            [AssignedTo(RoleId, ContainerType.Role)]
            [AssignedTo(FunctionId, ContainerType.Function)]
            [HasClientSettings("test", "{\"value\":1}")]
            [HasTag(TagId, false)]
            public const string UserId = "f2eb1741-ed9f-4819-890d-1f166cb94e41";

            [Profile(ProfileKind.Group)]
            public const string ParentGroupId = "e8d8dea7-eeee-4031-9823-0fa2370828dd";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "5e84e72b-f394-4cee-8f8d-4e9a073a0308";

            [Role(Name = "Test-Role")]
            public const string RoleId = "757f9ce4-2ab6-417f-9ea2-b77ce9c14d1c";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "b33419a5-ec78-4707-8048-d28726d92e64";

            [Tag(nameof(DeleteUser))]
            public const string TagId = "863fca9f-6cc0-4c71-b69d-577db62d4492";
        }

        public static class DeleteGroup
        {
            [Profile(ProfileKind.Group)]
            [AssignedTo(ParentGroupId, ContainerType.Group)]
            [AssignedTo(RoleId, ContainerType.Role)]
            [AssignedTo(FunctionId, ContainerType.Function)]
            [HasClientSettings("test", "{\"value\":1}")]
            [HasTag(TagId, false)]
            public const string TargetId = "0a76f29b-a7e2-40ea-a99a-8b0c709c1fb5";

            [Profile(ProfileKind.Group)]
            public const string ParentGroupId = "cb7cfd88-56f8-4a1c-95ae-dc7fad123189";

            [Profile(ProfileKind.User)]
            [AssignedTo(TargetId, ContainerType.Group)]
            public const string MemberId = "e9a34370d-8b99-4d3b-b0f2-89acd6fcdfc5";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "8a884662-2308-4ca8-ba86-f38ac75b2f5e";

            [Role(Name = "Test-Role")]
            public const string RoleId = "797581c8-730e-4283-91f9-541fcb58319e";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "2757d6f9-aa97-43ad-afe9-2732a1f9a85e";

            [Tag(nameof(DeleteGroup))]
            public const string TagId = "8eb18f47-c184-4859-ae04-54f52eb1d47c";
        }

        public static class DeleteSecondGroup
        {
            [Profile(ProfileKind.Group)]
            [AssignedTo(RootGroupId, ContainerType.Group, -60, 30)]
            [HasClientSettings("test", "{\"value\":1}")]
            public const string GroupId = "9117120c-6372-4cf4-9ac8-9ccb472239aa";

            [Profile(ProfileKind.Group)]
            public const string RootGroupId = "7d97db42-bb1e-4000-9f44-e8d7e952391f";

            [Profile(ProfileKind.User)]
            [AssignedTo(GroupId, ContainerType.Group, 120, 365)]
            public const string UserId = "fb54c265-f599-4858-bc6c-83db14dc1d24";
        }

        public static class DeleteOrganization
        {
            [Profile(ProfileKind.Organization)]
            [AssignedTo(ParentOrgId, ContainerType.Organization)]
            [HasTag(TagId, false)]
            public const string TargetId = "82f88248-f50f-4745-8cd1-9c4db7bb9659";

            [Profile(ProfileKind.Organization)]
            public const string ParentOrgId = "febdd793-cee4-43ae-8892-0adf5674ab9e";

            [Profile(ProfileKind.Organization)]
            [AssignedTo(TargetId, ContainerType.Organization)]
            public const string ChildOrgId = "8b353d9f-f5c2-4d5d-9984-01c54a401428";

            [Role(Name = "Test-Role")]
            public const string RoleId = "55df61d3-9017-4a05-9b09-9ac59f77c286";

            [Function(RoleId, TargetId)]
            public const string FunctionId = "35c78073-1533-449b-bcaf-07fde1d79735";

            [Tag(nameof(DeleteOrganization))]
            public const string TagId = "2facb1e8-09ad-4d37-98c3-d50a17d48171";
        }

        #endregion

        #region Update

        public static class UpdateUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "2220ff4a-32d0-403a-bfb7-1c2ae34575fb";
        }

        public static class UpdateGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "9ddcb0a1-5938-40ec-b01b-01a51314786f";
        }

        public static class UpdateOrganization
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "2586584d-457e-40a3-8faf-a8bf5212f226";
        }

        #endregion

        #region Tags

        public static class AddTagToUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "db196414-7f58-42f0-9acc-b8ff19ea2080";

            [Tag(nameof(AddTagToUser))]
            public const string TagId = "26271403-0cea-4678-befd-d553960dffc0";
        }

        public static class AddTagToGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "2797f184-e847-4b19-8b68-9fb361ede0f9";

            [Tag(nameof(AddTagToGroup))]
            public const string TagId = "0ba98e1b-b1d6-4b50-a12b-db39c48ada6b";
        }

        public static class AddTagToOrganization
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "568a93d7-a4a8-4952-b3e2-cbdfaea41e8b";

            [Tag(nameof(AddTagToOrganization))]
            public const string TagId = "58a2e242-4dbe-4c24-8d34-ee1cb90b4e21";
        }

        public static class AddNotExistingTagToUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "5e6c0985-767e-41d8-a620-93754dcce67d";
        }

        public static class AddNotExistingTagToGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "daa317df-50d6-4e22-9054-fdd898e3b67c";
        }

        public static class AddNotExistingTagToOrganization
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "999c0e86-5a42-4ee2-aead-c22bf36b6a77";
        }

        public static class AddTagToNotExistingProfile
        {
            [Tag(nameof(AddTagToNotExistingProfile))]
            public const string TagId = "95851a3e-c83a-40c8-bb57-2b320c18f811";
        }

        public static class RemoveTagFromUser
        {
            [Profile(ProfileKind.User)]
            [HasTag(TagId, true)]
            public const string ProfileId = "bb163baf-c698-44a1-b5cd-0ef4b5b008d4";

            [Tag(nameof(RemoveTagFromUser))]
            public const string TagId = "c6b49cc3-257f-428f-ad41-57b9e1239036";
        }

        public static class RemoveTagFromGroup
        {
            [Profile(ProfileKind.Group)]
            [HasTag(TagId, true)]
            public const string ProfileId = "1ec6a0d7-a698-4ba4-8863-4d2b4d470d0f";

            [Tag(nameof(RemoveTagFromGroup))]
            public const string TagId = "de694169-28b9-444e-be17-76f46f0b8d32";
        }

        public static class RemoveTagFromOrganization
        {
            [Profile(ProfileKind.Organization)]
            [HasTag(TagId, true)]
            public const string ProfileId = "b8783a8a-8482-4c79-a492-92d537b292da";

            [Tag(nameof(RemoveTagFromOrganization))]
            public const string TagId = "4a686c8d-4c54-4bfb-bb45-358166be1b92";
        }

        public static class RemoveTagFromNotExistingProfile
        {
            [Tag(nameof(RemoveTagFromNotExistingProfile))]
            public const string TagId = "434f66d3-baf3-4875-8628-559c84afca4a";
        }

        public static class RemoveNotExistingTagFromUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "64eb7aa6-3e7b-4cd8-89d5-efa2c382ea2a";
        }

        public static class RemoveNotExistingTagFromGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "58e4a401-b8a9-4d62-a66d-cc95fe754fe5";
        }

        public static class RemoveNotExistingTagFromOrganization
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "ee7ebe19-fed6-41b4-a3d3-7f162869f918";
        }

        public static class RemoveNotAssignedTagFromUser
        {
            [Profile(ProfileKind.User)]
            public const string UserId = "a2df6118-1956-4f53-8de9-1d90f445f4c3";

            [Tag(nameof(RemoveNotAssignedTagFromUser))]
            public const string TagId = "4a935e84-8427-4d4c-a5fb-bbab2ca796f1";
        }

        public static class RemoveNotAssignedTagFromGroup
        {
            [Profile(ProfileKind.Group)]
            public const string GroupId = "01038a18-888f-40ac-b09b-6ebe22dd40f4";

            [Tag(nameof(RemoveNotAssignedTagFromGroup))]
            public const string TagId = "e6d2b84f-5974-4ec7-93da-2fcd6ad9cd8e";
        }

        public static class RemoveNotAssignedTagFromOrganization
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "041ea799-4d58-4dcd-829b-ac5648a9ab54";

            [Tag(nameof(RemoveNotAssignedTagFromUser))]
            public const string TagId = "ace3c64c-3c51-448b-9cbf-2a44dd6d90b7";
        }

        #endregion
    }
}
