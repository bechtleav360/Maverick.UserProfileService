using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    [SeedData]
    public class RoleWriteTestData
    {
        public class DeleteRole
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(RoleId, ContainerType.Role)]
            [AssignedTo(TargetRoleId, ContainerType.Role)]
            public const string UserId = "8d5373c5-6fb9-4e02-92bb-f812f6d9086e";

            [Profile(ProfileKind.Organization)]
            public const string OrganizationId = "50138947-5752-4247-8463-13e3946e5b73";

            [Role(Name = "Test-Role")]
            [HasTag(TagId, false)]
            public const string RoleId = "e0e8bdc0-7e01-42ec-9cfa-5be8bd43eaf4";

            [Role(Name = "Target-Role")]
            [HasTag(TagId, false)]
            public const string TargetRoleId = "c4a7783d-7555-4943-a3df-3297c6917df2";

            [Function(RoleId, OrganizationId)]
            public const string FunctionAId = "43810300-ec2c-4be5-8eb5-f0ea30d292ef";

            [Function(TargetRoleId, OrganizationId)]
            public const string FunctionBId = "b62b1615-9884-4b8c-aba0-7d88c9f1dbb9";

            [Tag(nameof(DeleteRole))]
            public const string TagId = "8dd97778-aafd-4b02-b4f8-e3e2c902e286";
        }

        public class DeleteAnotherRole
        {
            [Profile(ProfileKind.User)]
            [AssignedTo(RoleId, ContainerType.Role, -30, 30)]
            [AssignedTo(TargetRoleId, ContainerType.Role, 120, 365)]
            public const string UserId = "614e7892-c880-40e4-a9bf-55bacbbaee87";

            [Role(Name = "Test-Role")]
            public const string RoleId = "e0e8bdc0-7e01-42ec-9cfa-5be8bd43eaf4";

            [Role(Name = "Deleted-Role")]
            public const string TargetRoleId = "f955dddd-cbda-4e36-9dde-a6209eb05d48";
        }

        public class UpdateRole
        {
            [Role(Name = "Test-Role")]
            public const string RoleId = "e0e8bdc0-7e01-42ec-afc9-5be8bd43eaf4";
        }

        public class AddTag
        {
            [Role(Name = "Test-Role")]
            public const string RoleId = "dc0e0e8b-7e01-42ec-afc9-5be8bd43eaf4";

            [Tag(nameof(AddTag))]
            public const string TagId = "8dd97778-aafd-4b02-b4f8-e3e2c902e286";
        }

        public class AddNotExistingTag
        {
            [Role(Name = "Test-Role")]
            public const string RoleId = "5be8bd8b-7e01-42ec-afc9-dc0e0e43eaf4";
        }

        public class AddTagToNotExistingRole
        {
            [Tag(nameof(AddTag))]
            public const string TagId = "aafd7778-8dd9-4b02-b4f8-e3e2c902e286";
        }

        public class RemoveTag
        {
            [Role(Name = "Test-Role")]
            [HasTag(TagId, true)]
            public const string RoleId = "3eaf4e8b-7e01-42ec-afc9-5be8bd4dc0e0";

            [Tag(nameof(AddTag))]
            public const string TagId = "b4f87778-aafd-4b02-8dd9-e3e2c902e286";
        }

        public class RemoveTagFromNotExistingRole
        {
            [Tag(nameof(RemoveTagFromNotExistingRole))]
            public const string TagId = "aa8dd978-fd77-b4f8-4b02-e3e2c902e286";
        }

        public class RemoveFromNotExistingTag
        {
            [Role(Name = "Test-Role")]
            public const string RoleId = "afc97e01-bd8b-42ec-5be8-dc0e0e43eaf4";
        }

        public class RemoveNotAssignedTag
        {
            [Role(Name = "Test-Role")]
            public const string RoleId = "bd4dce8b-7e01-42ec-afc9-5be80e03eaf4";

            [Tag(nameof(AddTag))]
            public const string TagId = "b4f87778-4b02-aafd-8dd9-e3e2c902e286";
        }
    }
}
