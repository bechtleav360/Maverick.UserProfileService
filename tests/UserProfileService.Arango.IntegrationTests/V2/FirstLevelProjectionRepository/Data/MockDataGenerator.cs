using System;
using System.Collections.Generic;
using Bogus;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    internal class MockDataGenerator
    {
        internal static FirstLevelProjectionUser GenerateUser(string id = null)
        {
            return new Faker<FirstLevelProjectionUser>()
                .RuleFor(u => u.Id, id ?? Guid.NewGuid().ToString("D"))
                .RuleFor(u => u.Name, faker => faker.Person.FirstName + "." + faker.Person.LastName)
                .RuleFor(u => u.FirstName, faker => faker.Person.FirstName)
                .RuleFor(u => u.LastName, faker => faker.Person.LastName)
                .RuleFor(u => u.DisplayName, faker => faker.Person.FullName)
                .RuleFor(u => u.UserName, faker => faker.Person.UserName)
                .RuleFor(u => u.Email, faker => faker.Person.Email)
                .RuleFor(u => u.CreatedAt, faker => faker.Date.Past().ToUniversalTime())
                .RuleFor(u => u.UpdatedAt, faker => faker.Date.Recent().ToUniversalTime())
                .RuleFor(
                    u => u.SynchronizedAt,
                    (faker, user)
                        => faker.Date.Between(user.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(
                    u => u.ExternalIds,
                    (_, user)
                        => user.SynchronizedAt != null
                            ? new List<ExternalIdentifier>
                            {
                                new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                            }
                            : new List<ExternalIdentifier>())
                .Generate();
        }

        internal static FirstLevelProjectionGroup GenerateGroup(string id = null)
        {
            return new Faker<FirstLevelProjectionGroup>()
                .RuleFor(g => g.Id, id ?? Guid.NewGuid().ToString("D"))
                .RuleFor(g => g.Name, faker => faker.Name.JobType())
                .RuleFor(g => g.DisplayName, faker => faker.Name.JobTitle())
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past().ToUniversalTime())
                .RuleFor(g => g.UpdatedAt, faker => faker.Date.Recent().ToUniversalTime())
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(
                    g => g.ExternalIds,
                    (_, user)
                        => user.SynchronizedAt != null
                            ? new List<ExternalIdentifier>
                            {
                                new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                            }
                            : new List<ExternalIdentifier>())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool())
                .RuleFor(g => g.IsMarkedForDeletion, faker => faker.Random.Bool(0.3F))
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 10))
                .Generate();
        }

        internal static FirstLevelProjectionOrganization GenerateOrganization(string id = null)
        {
            return new Faker<FirstLevelProjectionOrganization>()
                .RuleFor(g => g.Id, id ?? Guid.NewGuid().ToString("D"))
                .RuleFor(g => g.Name, faker => faker.Name.JobType())
                .RuleFor(g => g.DisplayName, faker => faker.Name.JobTitle())
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past().ToUniversalTime())
                .RuleFor(g => g.UpdatedAt, faker => faker.Date.Recent().ToUniversalTime())
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(
                    g => g.ExternalIds,
                    (_, user)
                        => user.SynchronizedAt != null
                            ? new List<ExternalIdentifier>
                            {
                                new ExternalIdentifier(Guid.NewGuid().ToString(), "test")
                            }
                            : new List<ExternalIdentifier>())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool())
                .RuleFor(g => g.IsMarkedForDeletion, faker => faker.Random.Bool(0.3F))
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 10))
                .Generate();
        }

        internal static FirstLevelProjectionRole GenerateRole(string id = null)
        {
            return new Faker<FirstLevelProjectionRole>()
                .RuleFor(r => r.Id, id ?? Guid.NewGuid().ToString("D"))
                .RuleFor(r => r.Name, faker => faker.Name.JobArea())
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool(0.3F))
                .RuleFor(r => r.Description, faker => faker.Lorem.Slug(5))
                .RuleFor(r => r.Permissions, faker => faker.Lorem.Words(5))
                .Generate();
        }

        internal static FirstLevelProjectionFunction GenerateFunction(
            string id = null,
            FirstLevelProjectionRole role = null,
            FirstLevelProjectionOrganization organization = null)
        {
            role ??= new FirstLevelProjectionRole();
            organization ??= new FirstLevelProjectionOrganization();

            return new Faker<FirstLevelProjectionFunction>()
                .RuleFor(f => f.Id, id ?? Guid.NewGuid().ToString("D"))
                .RuleFor(f => f.CreatedAt, faker => faker.Date.Past().ToUniversalTime())
                .RuleFor(f => f.UpdatedAt, faker => faker.Date.Recent().ToUniversalTime())
                .RuleFor(f => f.Role, _ => role)
                .RuleFor(f => f.Organization, _ => organization)
                .Generate();
        }
    }
}
