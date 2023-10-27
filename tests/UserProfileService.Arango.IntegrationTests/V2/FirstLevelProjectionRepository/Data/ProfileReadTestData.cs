using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    public class ProfileReadTestData : ITestData
    {
        public static FirstLevelProjectionUser ReadUser =
            new FirstLevelProjectionUser
            {
                Id = Guid.NewGuid().ToString(),
                Source = "Test-Source",
                CreatedAt = DateTime.MinValue,
                Name = "Test-Read-User",
                SynchronizedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.Today,
                ExternalIds = new List<ExternalIdentifier>
                {
                    new ExternalIdentifier("ext-id-user", "source"),
                    new ExternalIdentifier("ext-id-converted-user", "source", true)
                },
                Email = "test@bechtle.com",
                DisplayName = "Test-User-Display-Name",
                FirstName = "Test-User",
                LastName = "Name",
                UserName = "Test-User-User-Name",
                UserStatus = "ExampleStatus"
            };

        public static FirstLevelProjectionGroup ReadGroup =
            new FirstLevelProjectionGroup
            {
                Id = Guid.NewGuid().ToString(),
                Source = "Test-Source",
                CreatedAt = DateTime.MinValue,
                Name = "Test-Read-Group",
                SynchronizedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.Today,
                ExternalIds = new List<ExternalIdentifier>
                {
                    new ExternalIdentifier("ext-id-group", "source"),
                    new ExternalIdentifier("ext-id-converted-group", "source", true)
                },
                DisplayName = "Test-Group-Display-Name",
                IsMarkedForDeletion = true,
                IsSystem = true,
                Weight = Math.PI
            };

        public static FirstLevelProjectionOrganization ReadOrganization =
            new FirstLevelProjectionOrganization
            {
                Id = Guid.NewGuid().ToString(),
                Source = "Test-Source",
                CreatedAt = DateTime.MinValue,
                Name = "Test-Read-Organization",
                SynchronizedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.Today,
                ExternalIds = new List<ExternalIdentifier>
                {
                    new ExternalIdentifier("ext-id-group", "source"),
                    new ExternalIdentifier("ext-id-converted-group", "source") //TODO check isConverted
                },
                DisplayName = "Test-Organization-Display-Name",
                IsMarkedForDeletion = true,
                IsSystem = true,
                Weight = Math.PI,
                IsSubOrganization = true
            };

        /// <inheritdoc />
        public string Name => nameof(ProfileReadTestData);

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionRole>> Roles { get; } =
            new List<ExtendedEntity<FirstLevelProjectionRole>>();

        /// <inheritdoc />
        public IList<ExtendedProfile> Profiles { get; } = new List<ExtendedProfile>
        {
            new ExtendedProfile
            {
                Value = ReadUser
            },
            new ExtendedProfile
            {
                Value = ReadGroup
            },
            new ExtendedProfile
            {
                Value = ReadOrganization
            }
        };

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
