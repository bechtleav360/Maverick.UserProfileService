using System;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    public class RoleAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
