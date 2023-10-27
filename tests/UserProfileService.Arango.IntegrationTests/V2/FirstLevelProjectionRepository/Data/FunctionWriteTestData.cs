using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    [SeedData]
    public static class FunctionWriteTestData
    {
        public static class CreateFunction
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "9e233c07-1c56-45c3-8527-82e520897f46";

            [Role(Name = "Test-Role")]
            public const string RoleId = "3566046c-1fde-4cdf-803d-9f0e3f1f9ef2";
        }

        public static class DeleteFunction
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(RoleId, ContainerType.Role)]
            [AssignedTo(TargetFunctionId, ContainerType.Role)]
            public const string UserId = "0904bab7-622c-4104-9874-66248af5c282";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "fab9c5e1-d1ed-44ec-adf0-a40a8c66a591";

            [Role(Name = "Test-Role")]
            [HasTag(TagId, false)]
            public const string RoleId = "32b11500-da3e-4d25-81da-8cd99cb2b1ea";

            [Function(RoleId, OrganizationId)]
            [HasTag(TagId, false)]
            public const string TargetFunctionId = "9d091009-a301-4ca5-be52-7154b1a8a4c0";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "66709056-e5ed-4034-b92c-b5a578fecbe6";

            [Tag(nameof(DeleteFunction))]
            public const string TagId = "4a444507-8045-476a-a687-27277a862716";
        }

        public static class DeleteAnotherFunction
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(RoleId, ContainerType.Role, 30, 60)]
            [AssignedTo(TargetFunctionId, ContainerType.Role, 45, 90)]
            public const string UserId = "cff665f6-a730-411d-b39b-00f2def04ccb";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "31f36443-c23c-42c2-a80e-2070540e3918";

            [Role(Name = "Test-Role")]
            public const string RoleId = "32b11500-da3e-4d25-81da-8cd99cb2b1ea";

            [Function(RoleId, OrganizationId)]
            public const string TargetFunctionId = "c434f6e0-022b-4ba2-9c6f-f2ee9d6424f6";
        }

        public static class UpdateFunction
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "be0df51c-967c-4f74-89ba-bc96bd69d095";

            [Role(Name = "Test-Role")]
            public const string RoleId = "60d8a0fb-cca1-4565-942a-ae8dd2e249e7";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "ee9343b7-dbd2-457e-9694-d81e134be5bb";
        }

        public static class AddTag
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "46225fdb-3850-4499-b28a-eb53f55769d9";

            [Role(Name = "Test-Role")]
            public const string RoleId = "1d1696c7-1a0e-425a-871a-2302fd90a61e";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "605aebba-b116-473e-96a4-720047ae93f0";

            [Tag(nameof(AddTag))]
            public const string TagId = "49ddbb4a-da87-4028-9cc9-97252189da1c";
        }

        public static class AddNotExistingTag
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "1145f599-ef2f-4e4e-a4d5-4302a5febed6";

            [Role(Name = "Test-Role")]
            public const string RoleId = "9814e8c1-448d-4763-873d-fcf2dd34ec97";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "73590f59-7c51-4abd-a882-60f05dbe688b";
        }

        public static class AddTagToNotExistingFunction
        {
            [Tag(nameof(AddTag))]
            public const string TagId = "070cb4d8-2571-4c15-b546-4162acf8be93";
        }

        public static class RemoveTag
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "51a112e6-c440-471c-a86f-d78ad4593776";

            [Role(Name = "Test-Role")]
            public const string RoleId = "0710536a-5194-4396-b13d-eeb4ef7897d8";

            [Function(RoleId, OrganizationId)]
            [HasTag(TagId, true)]
            public const string FunctionId = "daae32c8-2d3f-4366-bf9d-0450c56522a8";

            [Tag(nameof(AddTag))]
            public const string TagId = "ccf98177-c35d-4812-a31e-b34b24f16f3f";
        }

        public static class RemoveTagFromNotExistingFunction
        {
            [Tag(nameof(RemoveTagFromNotExistingFunction))]
            public const string TagId = "a33d023c-a5ee-4927-b5b0-86bf705039fc";
        }

        public static class RemoveNotExistingTag
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "7d3e54b6-b715-4611-8e6d-b11ea8cdc0b9";

            [Role(Name = "Test-Role")]
            public const string RoleId = "8a47e59c-2081-4645-9539-d4392fd2b6b6";

            [Function(RoleId, OrganizationId)]
            public const string FunctionId = "1f867b0f-ce4f-46f5-9b7a-67bf31f80e3e";
        }

        public static class RemoveNotAssignedTag
        {
            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "fcdae04d-c4bf-4b7f-b807-6eb35d7388b2";

            [Role(Name = "Test-Role")]
            public const string RoleId = "dc36c526-b987-479e-8def-5452a10c94c1";

            [Function(RoleId, OrganizationId)]
            [HasTag(TagId, true)]
            public const string FunctionId = "b71d734c-490d-4127-9fbc-6a0d33ca3ebd";

            [Tag(nameof(AddTag))]
            public const string TagId = "c2656db3-27d1-4836-a144-dc9da37111cb";
        }
    }
}
