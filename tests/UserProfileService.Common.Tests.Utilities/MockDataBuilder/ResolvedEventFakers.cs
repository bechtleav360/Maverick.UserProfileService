using System.Collections.Generic;
using System.Linq;
using Bogus;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    public static class ResolvedEventFakers
    {
        public static Faker<FunctionCreated> NewFunctionCreated =>
            new Faker<FunctionCreated>()
                .RuleFor(
                    e => e.Organization,
                    _ => ResolvedEventsModelFakers.NewOrganization.Generate(1).Single())
                .RuleFor(
                    e => e.Role,
                    _ => ResolvedEventsModelFakers.NewRole.Generate(1).Single())
                .RuleFor(e => e.EventId, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    e => e.Tags,
                    faker =>
                        ResolvedEventsModelFakers.NewTagAssignment.Generate(faker.Random.Int(0, 3)).ToArray())
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.Source, faker => faker.Random.Word())
                .RuleFor(e => e.Type, _ => nameof(FunctionCreated))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    })
                .RuleFor(
                    e => e.EventId,
                    faker => faker.Random.Guid().ToString());

        public static Faker<GroupCreated> NewGroupCreated =>
            new Faker<GroupCreated>()
                .RuleFor(e => e.Name, faker => faker.Person.UserName)
                .RuleFor(
                    e => e.DisplayName,
                    faker => faker.Person.FullName)
                .RuleFor(
                    e => e.Weight,
                    faker => faker.Random.Int(0, 1000))
                .RuleFor(e => e.EventId, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    e => e.Tags,
                    faker =>
                        ResolvedEventsModelFakers.NewTagAssignment.Generate(faker.Random.Int(0, 3)).ToArray())
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.Source, faker => faker.Random.Word())
                .RuleFor(e => e.Type, _ => nameof(GroupCreated))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    });

        public static Faker<OrganizationCreated> NewOrganizationCreated =>
            new Faker<OrganizationCreated>()
                .RuleFor(e => e.Name, faker => faker.Person.UserName)
                .RuleFor(
                    e => e.DisplayName,
                    faker => faker.Person.FullName)
                .RuleFor(
                    e => e.Weight,
                    faker => faker.Random.Int(0, 1000))
                .RuleFor(e => e.EventId, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    e => e.Tags,
                    faker =>
                        ResolvedEventsModelFakers.NewTagAssignment.Generate(faker.Random.Int(0, 3)).ToArray())
                .RuleFor(e => e.IsSubOrganization, faker => faker.Random.Bool(0.1F))
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.Source, faker => faker.Random.Word())
                .RuleFor(e => e.Type, _ => nameof(OrganizationCreated))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    });

        public static Faker<RoleCreated> NewRoleCreated =>
            new Faker<RoleCreated>()
                .RuleFor(
                    r => r.Name,
                    faker => faker.Commerce.Department(1))
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
                            faker.Random.String(3, 15))
                    })
                .RuleFor(e => e.EventId, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    e => e.Tags,
                    faker =>
                        ResolvedEventsModelFakers.NewTagAssignment.Generate(faker.Random.Int(0, 3)).ToArray())
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.Source, faker => faker.Random.Word())
                .RuleFor(e => e.Type, _ => nameof(FunctionCreated))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    })
                .RuleFor(
                    e => e.EventId,
                    faker => faker.Random.Guid().ToString());

        public static Faker<UserCreated> NewUserCreated =>
            new Faker<UserCreated>()
                .RuleFor(e => e.Name, faker => faker.Person.UserName)
                .RuleFor(
                    e => e.DisplayName,
                    faker => faker.Person.FullName)
                .RuleFor(e => e.FirstName, faker => faker.Person.FirstName)
                .RuleFor(e => e.LastName, faker => faker.Person.LastName)
                .RuleFor(e => e.Email, faker => faker.Person.Email)
                .RuleFor(
                    e => e.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.EventId, faker => faker.Random.Guid().ToString())
                .RuleFor(e => e.Source, faker => faker.Random.Word())
                .RuleFor(e => e.Type, _ => nameof(UserCreated))
                .RuleFor(
                    e => e.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            faker.System.AndroidId(),
                            faker.Random.String(3, 15))
                    });
    }
}
