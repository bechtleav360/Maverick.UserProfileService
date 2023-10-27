using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    public class TagReadTestData : ITestData
    {
        public static FirstLevelProjectionTag ReadTag = new FirstLevelProjectionTag
        {
            Id = Guid.NewGuid().ToString("D"),
            Name = "Read-Test-Tag",
            Type = TagType.Custom
        };

        /// <inheritdoc />
        public string Name => nameof(RoleReadTestData);

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionRole>> Roles { get; } =
            new List<ExtendedEntity<FirstLevelProjectionRole>>();

        /// <inheritdoc />
        public IList<ExtendedProfile> Profiles { get; } = new List<ExtendedProfile>();

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionFunction>> Functions { get; } =
            new List<ExtendedEntity<FirstLevelProjectionFunction>>();

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionTag>> Tags { get; } =
            new List<ExtendedEntity<FirstLevelProjectionTag>>
            {
                new ExtendedEntity<FirstLevelProjectionTag>
                {
                    Value = ReadTag
                }
            };

        public IList<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>> TemporaryAssignments { get; } =
            new List<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>>();
    }
}
