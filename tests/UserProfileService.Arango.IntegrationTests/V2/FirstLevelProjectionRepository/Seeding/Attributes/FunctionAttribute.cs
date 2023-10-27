using System;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    public class FunctionAttribute : Attribute
    {
        public string RoleId { get; set; }
        public string OrganizationId { get; set; }

        public FunctionAttribute(string roleId, string organizationId)
        {
            RoleId = roleId;
            OrganizationId = organizationId;
        }
    }
}
