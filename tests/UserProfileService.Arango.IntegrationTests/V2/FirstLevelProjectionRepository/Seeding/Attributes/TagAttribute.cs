using System;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    public class TagAttribute : Attribute
    {
        public string Name { get; set; }

        public TagAttribute(string name)
        {
            Name = name;
        }
    }
}
