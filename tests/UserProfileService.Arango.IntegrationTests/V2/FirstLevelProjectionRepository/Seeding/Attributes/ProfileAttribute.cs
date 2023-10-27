using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    public class ProfileAttribute : Attribute
    {
        public ProfileKind Kind { get; set; }
        public string Name { get; set; }

        public ProfileAttribute(ProfileKind kind)
        {
            Kind = kind;
        }
    }
}
