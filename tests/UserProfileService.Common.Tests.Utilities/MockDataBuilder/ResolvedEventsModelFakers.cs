using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    public static class ResolvedEventsModelFakers
    {
        public static Faker<Function> NewFunction =>
            new Faker<Function>()
                .RuleFor(f => f.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(f => f.Source, faker => faker.Random.Word())
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, group) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    r => r.SynchronizedAt,
                    faker
                        => faker.Date.Recent()
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(f => f.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    f => f.UpdatedAt,
                    (
                        faker,
                        f) => faker.Date.Between(f.CreatedAt, DateTime.UtcNow))
                .RuleFor(
                    f => f.Organization,
                    faker => NewOrganization.Generate(1).Single())
                .RuleFor(
                    f => f.OrganizationId,
                    (faker, function) => function.Organization.Id)
                .RuleFor(f => f.Role, faker => NewRole.Generate(1).Single())
                .RuleFor(f => f.RoleId, (faker, f) => f.Role.Id);

        public static Faker<Group> NewGroup =>
            new Faker<Group>()
                .RuleFor(
                    o => o.Name,
                    faker => faker.Company.CompanyName(0))
                .RuleFor(
                    e => e.DisplayName,
                    faker => faker.Company.CompanyName(2))
                .RuleFor(
                    o => o.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    r => r.CreatedAt,
                    faker => faker.Date.Past())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, r)
                        => faker.Date.Between(r.CreatedAt, DateTime.UtcNow))
                .RuleFor(
                    r => r.UpdatedAt,
                    (faker, r)
                        => faker.Date.Between(
                            r.SynchronizedAt
                            ?? r.CreatedAt,
                            DateTime.UtcNow))
                .RuleFor(o => o.ContainerType, _ => ContainerType.Organization)
                .RuleFor(
                    o => o.Weight,
                    faker => faker.Random.Int(0, 1000))
                .RuleFor(
                    o => o.Kind,
                    _ => ProfileKind.Organization)
                .RuleFor(
                    o => o.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    });

        public static Faker<Organization> NewOrganization =>
            new Faker<Organization>()
                .RuleFor(
                    o => o.Name,
                    faker => faker.Company.CompanyName(0))
                .RuleFor(
                    e => e.DisplayName,
                    faker => faker.Company.CompanyName(2))
                .RuleFor(
                    o => o.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    r => r.CreatedAt,
                    faker => faker.Date.Past())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, r)
                        => faker.Date.Between(r.CreatedAt, DateTime.UtcNow))
                .RuleFor(
                    r => r.UpdatedAt,
                    (faker, r)
                        => faker.Date.Between(
                            r.SynchronizedAt
                            ?? r.CreatedAt,
                            DateTime.UtcNow))
                .RuleFor(o => o.ContainerType, _ => ContainerType.Organization)
                .RuleFor(
                    o => o.IsSubOrganization,
                    faker => faker.Random.Bool(0.2F))
                .RuleFor(
                    o => o.Weight,
                    faker => faker.Random.Int(0, 1000))
                .RuleFor(
                    o => o.Kind,
                    _ => ProfileKind.Organization)
                .RuleFor(
                    o => o.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Database.Engine())
                    })
                .RuleFor(
                    o => o.TagUrl,
                    faker => faker.Internet.UrlWithPath("https"))
                .RuleFor(
                    o => o.Source,
                    faker => faker.Database.Engine());

        public static Faker<Role> NewRole =>
            new Faker<Role>()
                .RuleFor(
                    r => r.Name,
                    faker => faker.Commerce.Department(1))
                .RuleFor(
                    r => r.ContainerType,
                    _ => ContainerType.Role)
                .RuleFor(
                    r => r.CreatedAt,
                    faker => faker.Date.Past())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, r)
                        => faker.Date.Between(r.CreatedAt, DateTime.UtcNow))
                .RuleFor(
                    r => r.UpdatedAt,
                    (faker, r)
                        => faker.Date.Between(
                            r.SynchronizedAt
                            ?? r.CreatedAt,
                            DateTime.UtcNow))
                .RuleFor(
                    r => r.Description,
                    faker => faker.Lorem.Paragraphs(2))
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    e => e.Source,
                    faker => faker.Random.Word())
                .RuleFor(
                    r => r.IsSystem,
                    faker => faker.Random.Bool(0.33F))
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Random.WordsArray(1, 5))
                .RuleFor(
                    r => r.Permissions,
                    faker => faker.Random.WordsArray(1, 5))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Database.Engine())
                    });

        public static Faker<TagAssignment> NewTagAssignment =>
            new Faker<TagAssignment>()
                .RuleFor(ta => ta.IsInheritable, _ => true)
                .RuleFor(
                    ta => ta.TagDetails,
                    faker => new Tag
                    {
                        Id = faker.Random.Guid().ToString(),
                        Name = faker.Random.Word(),
                        Type = TagType.Custom
                    });
    }
}
