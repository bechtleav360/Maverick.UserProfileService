using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    public class RoleReadTestData : ITestData
    {
        public static FirstLevelProjectionRole ReadRole =
            new FirstLevelProjectionRole
            {
                Id = Guid.NewGuid().ToString(),
                Source = "Test-Source",
                CreatedAt = DateTime.MinValue,
                Name = "Test-Read-Rolle",
                DeniedPermissions = new List<string>
                {
                    "Denied-A",
                    "Denied-B"
                },
                Permissions = new List<string>
                {
                    "A",
                    "B"
                },
                SynchronizedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.Today,
                ExternalIds = new List<ExternalIdentifier>
                {
                    new ExternalIdentifier("ext-id", "source"),
                    new ExternalIdentifier("ext-id-converted", "source", true)
                },
                Description = "Lorem Ipsum dolor äöü+#.,´´0ß=;,:.",
                IsSystem = true
            };

        /// <inheritdoc />
        public string Name => nameof(RoleReadTestData);

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionRole>> Roles { get; } =
            new List<ExtendedEntity<FirstLevelProjectionRole>>
            {
                new ExtendedEntity<FirstLevelProjectionRole>
                {
                    Value = ReadRole
                }
            };

        /// <inheritdoc />
        public IList<ExtendedProfile> Profiles { get; } = new List<ExtendedProfile>();

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionFunction>> Functions { get; } =
            new List<ExtendedEntity<FirstLevelProjectionFunction>>();

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionTag>> Tags { get; } =
            new List<ExtendedEntity<FirstLevelProjectionTag>>();

        public IList<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>> TemporaryAssignments { get; } =
            new List<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>>();
    }
}
