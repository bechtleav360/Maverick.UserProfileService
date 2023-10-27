using System;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TagAttribute : Attribute
    {
        public string Name { get; }

        public TagType? TagType { get; }

        public TagAttribute(string name)
        {
            Name = name;
            TagType = null;
        }

        public TagAttribute(string name, TagType tagType)
        {
            Name = name;
            TagType = tagType;
        }
    }
}
