using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bogus;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using AggregateProfileKind = Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;
using OrganizationAggregate = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using ExternalIdentifierAggregate = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using AggregatedModels = Maverick.UserProfileService.AggregateEvents.Common;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using TagType = Maverick.UserProfileService.Models.EnumModels.TagType;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     A static class to generate test data
    /// </summary>
    public static class MockDataGenerator
    {
        private static readonly string[] _exampleExternalIdSources =
        {
            "bonnea",
            "active-directory",
            "ms-graph-api",
            "salesForce",
            "PayPal",
            "Google"
        };

        /// <summary>
        ///     The source name where the entity was transferred to
        /// </summary>
        private static readonly string[] _sources = { "API", "Active directory" };

        private static Faker<TFunc> ApplyFunctionBasicRules<TFunc>(this Faker<TFunc> sourceFaker)
            where TFunc : FunctionBasic
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            return sourceFaker
                .RuleFor(f => f.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(f => f.Source, faker => faker.PickRandom(_sources))
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
                    faker => GenerateOrganizationInstances().FirstOrDefault())
                .RuleFor(
                    f => f.OrganizationId,
                    (faker, function) => function.Organization.Id)
                .RuleFor(f => f.Role, faker => GenerateRoleBasicInstances().Single())
                .RuleFor(f => f.RoleId, (faker, f) => f.Role.Id)
                // function name is a concatenated string of organization and role names
                .RuleFor(
                    f => f.Name,
                    (_, f)
                        => $"{f.Organization.Name} {f.Role.Name}");
        }

        private static Faker<FirstLevelProjectionFunction> ApplyFirstLevelProjectionFunctionRules(
            this Faker<FirstLevelProjectionFunction> sourceFaker,
            FirstLevelProjectionRole role = null,
            string id = null)
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            return sourceFaker
                .RuleFor(f => f.Id, faker => id ?? faker.Random.Guid().ToString())
                .RuleFor(f => f.Source, faker => faker.PickRandom(_sources))
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
                    faker => GenerateFirstLevelProjectionOrganizationInstances().FirstOrDefault())
                .RuleFor(f => f.Role, faker => role ?? GenerateFirstLevelRoles().Single());
        }

        private static Faker<TGroup> ApplyGroupBasicRules<TGroup>(this Faker<TGroup> sourceFaker)
            where TGroup : GroupBasic
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            return sourceFaker
                .RuleFor(g => g.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, group) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                .RuleFor(
                    g => g.UpdatedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    (
                        faker,
                        group) => !group.IsSystem && faker.Random.Bool(0.2f))
                .RuleFor(g => g.ImageUrl, (faker, u) => "/users/" + u.Id + "/image.png")
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, ProfileKind.Group)
                .RuleFor(
                    g => g.Name,
                    (
                        faker,
                        group) => group.DisplayName.Replace(" ", ":"))
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(g => g.TagUrl, (f, u) => "/groups/" + u.Id + "/tagUrl");
        }

        private static Faker<TLinkedObject> ApplyLinkedObjectRules<TLinkedObject>(this Faker<TLinkedObject> sourceFaker)
            where TLinkedObject : class, ILinkedObject
        {
            return sourceFaker
                .RuleFor(
                    f => f.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    f => f.Name,
                    faker => faker.Company.CompanyName(2))
                .RuleFor(
                    f => f.Conditions,
                    faker => GenerateRangeConditions(
                        faker.Random.Bool(0.8F)
                            ? 1
                            : faker.Random.Int(2, 5)));
        }

        private static Faker<TOrganization> ApplyOrganizationBasicRules<TOrganization>(
            this Faker<TOrganization> sourceFaker)
            where TOrganization : OrganizationBasic
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            return sourceFaker
                .RuleFor(o => o.Name, faker => "Z" + faker.Random.Int(1, 50))
                .RuleFor(o => o.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, organization) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{organization.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                .RuleFor(
                    g => g.UpdatedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    (
                        faker,
                        organization) => !organization.IsSystem && faker.Random.Bool(0.2f))
                .RuleFor(g => g.ImageUrl, (faker, u) => "/organizations/" + u.Id + "/image.png")
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, ProfileKind.Organization)
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(g => g.TagUrl, (f, u) => "/organizations/" + u.Id + "/tagUrl")
                .RuleFor(
                    o => o.IsSubOrganization,
                    f => f.Random.Bool(0.1F));
        }

        private static Faker<FirstLevelProjectionOrganization> ApplyFirstLevelProjectionOrganizationRules(
            this Faker<FirstLevelProjectionOrganization> sourceFaker)
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            return sourceFaker
                .RuleFor(o => o.Name, faker => "Z" + faker.Random.Int(1, 50))
                .RuleFor(o => o.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, organization) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{organization.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                .RuleFor(
                    g => g.UpdatedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    (
                        faker,
                        organization) => !organization.IsSystem && faker.Random.Bool(0.2f))
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, ProfileKind.Organization)
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(
                    o => o.IsSubOrganization,
                    f => f.Random.Bool(0.1F));
        }

        private static Faker<TProfile> ApplySecondLevelProfileRules<TProfile>(
            this Faker<TProfile> sourceFaker,
            AggregateProfileKind[] possibleMemberOfKinds,
            int minimumMemberOfItems = 0,
            int maximumMemberOfItems = 3)
            where TProfile : class, ISecondLevelProjectionProfile
        {
            return sourceFaker
                .RuleFor(o => o.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(o => o.DisplayName, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    p => p.ExternalIds,
                    (faker, profile) => faker.Make(
                        3,
                        () => new ExternalIdentifierAggregate(
                            $"{profile.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    p => p.Source,
                    faker => faker.Random.Word())
                .RuleFor(
                    p => p.MemberOf,
                    faker => GenerateAggregateResolvedEventMember(
                        faker.Random.Int(minimumMemberOfItems, maximumMemberOfItems),
                        possibleMemberOfKinds))
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    p => p.UpdatedAt,
                    (faker, profile)
                        => faker.Date.Between(profile.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime())
                .RuleFor(
                    p => p.SynchronizedAt,
                    (faker, profile)
                        => faker.Date.Between(profile.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                // at this point the path is not complete
                .RuleFor(
                    p => p.Paths,
                    (_, p) => new List<string>
                    {
                        p.Id
                    });
        }

        private static Faker<TFirstLevelUser> ApplyFirstLevelProfileRules<TFirstLevelUser>(
            this Faker<TFirstLevelUser> sourceFaker)
            where TFirstLevelUser : class, IFirstLevelProjectionProfile
        {
            return sourceFaker
                .RuleFor(o => o.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, organization) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{organization.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    p => p.Source,
                    faker => faker.Random.Word())
                .RuleFor(g => g.CreatedAt, _ => DateTime.UtcNow.ToLocalTime())
                .RuleFor(
                    g => g.UpdatedAt,
                    (faker, u) => u.CreatedAt.ToLocalTime())
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f));
        }

        private static Faker<TRole> ApplyRoleBasicRules<TRole>(this Faker<TRole> sourceFaker)
            where TRole : RoleBasic
        {
            return sourceFaker
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Source, faker => faker.PickRandom(_sources))
                .RuleFor(r => r.Name, faker => faker.Hacker.Verb())
                .RuleFor(r => r.Description, faker => faker.Lorem.Paragraph())
                .RuleFor(
                    r => r.ExternalIds,
                    (faker, role) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{role.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    r => r.Permissions,
                    faker =>
                        Enumerable
                            .Range(0, faker.Random.Int(0, 20))
                            .Select(item => faker.Hacker.Abbreviation())
                            .ToList()
                            .OrNull(faker, 0.1f))
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Random.WordsArray(0, 5).OrNull(faker, 0.1F))
                .RuleFor(r => r.Type, faker => RoleType.Role)
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool())
                .RuleFor(r => r.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    r => r.UpdatedAt,
                    (
                        faker,
                        role) => faker.Date.Between(role.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, role)
                        => faker.Date.Between(role.CreatedAt, role.UpdatedAt)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f));
        }

        private static Faker<TUser> ApplyUserBasicRules<TUser>(this Faker<TUser> sourceFaker)
            where TUser : UserBasic
        {
            if (sourceFaker == null)
            {
                throw new ArgumentNullException(nameof(sourceFaker));
            }

            var userStatus = new[] { "active", "not active", "sick" };

            return sourceFaker.RuleFor(u => u.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(u => u.Source, faker => faker.PickRandom(_sources))
                .RuleFor(
                    u => u.ExternalIds,
                    faker => faker.Make(
                        3,
                        () => new ExternalIdentifier(faker.System.AndroidId(), faker.Database.Engine())))
                .RuleFor(u => u.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    u => u.UpdatedAt,
                    (
                        faker,
                        group) => faker.Date.Between(group.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(x => x.ImageUrl, (f, u) => "/users/" + u.Id + "/image")
                .RuleFor(u => u.FirstName, faker => faker.Name.FirstName())
                .RuleFor(u => u.LastName, faker => faker.Name.LastName())
                .RuleFor(
                    u => u.Name,
                    (faker, user)
                        => $"{user.FirstName} {user.LastName}")
                .RuleFor(
                    u => u.DisplayName,
                    (
                            faker,
                            user) => $"{user.LastName}, {user.FirstName}")
                .RuleFor(
                    u => u.UserName,
                    (
                            faker,
                            user) => $"{user.FirstName[0]}{faker.Random.Int(1, 3)}.{user.LastName}")
                .RuleFor(
                    u => u.SynchronizedAt,
                    (faker, user)
                        => faker.Date.Between(user.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(
                    u => u.Email,
                    (faker, user)
                        => faker.Internet.Email(user.FirstName, user.LastName))
                .RuleFor(u => u.Kind, ProfileKind.User)
                .RuleFor(u => u.UserStatus, faker => faker.PickRandom(userStatus))
                .RuleFor(x => x.TagUrl, (f, u) => "/users/" + u.Id + "/tags");
        }

        private static List<Member> GenerateMembers(
            int number = 1,
            params ProfileKind[] possibleMemberKinds)
        {
            return new Faker<Member>().RuleFor(f => f.Id, fak => fak.Random.Guid().ToString())
                .RuleFor(
                    f => f.Kind,
                    faker => faker.PickRandom(
                        possibleMemberKinds != null && possibleMemberKinds.Length > 0
                            ? possibleMemberKinds
                            : new[] { ProfileKind.User, ProfileKind.Group, ProfileKind.Organization }))
                .RuleFor(f => f.Name, faker => faker.Company.CompanyName())
                .Generate(number);
        }

        private static List<ResolvedModels.Member>
            GenerateAggregateResolvedEventMember(
                int number,
                params AggregateProfileKind[] possibleMemberKinds)
        {
            return new Faker<ResolvedModels.Member>()
                .RuleFor(
                    m => m.Kind,
                    faker => faker.PickRandom(
                        possibleMemberKinds != null && possibleMemberKinds.Length > 0
                            ? possibleMemberKinds
                            : new[]
                            {
                                AggregateProfileKind.User,
                                AggregateProfileKind.Group,
                                AggregateProfileKind.Organization
                            }))
                .RuleFor(
                    m => m.Name,
                    (faker, m)
                        => m.Kind == AggregateProfileKind.User
                            ? faker.Person.UserName
                            : faker.Company.CompanyName())
                .RuleFor(m => m.DisplayName, (_, m) => m.Name)
                .RuleFor(m => m.ExternalIds, _ => new List<ExternalIdentifierAggregate>())
                .RuleFor(
                    m => m.Conditions,
                    faker => GenerateAggregateRangeConditions(faker.Random.Int(1, 2)))
                .Generate(number);
        }

        private static ObjectType ObjectTypeAssignments(ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.Group => new[] { ObjectType.Group, ObjectType.User }.PickRandom(),
                ObjectType.Organization => ObjectType.Organization,
                ObjectType.Function => new[] { ObjectType.Group, ObjectType.User }.PickRandom(),
                ObjectType.Role => new[] { ObjectType.Group, ObjectType.User }.PickRandom(),
                _ => throw new ArgumentOutOfRangeException($"Unknown type to match. Type {objectType}.")
            };
        }

        public static List<RangeCondition> GenerateRangeConditions(
            int number,
            float nullWeight = 0.75F)
        {
            return new Faker<RangeCondition>()
                .RuleFor(
                    c => c.Start,
                    faker => faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(3))
                        .OrNull(faker, nullWeight))
                .RuleFor(
                    c => c.End,
                    (faker, current) =>
                        current?.Start != null
                            ? faker.Date.Between(current.Start.Value, DateTime.UtcNow.AddMonths(6)) as DateTime?
                            : null)
                .Generate(number);
        }

        public static List<AggregateModels.RangeCondition> GenerateRangeConditionsAggregated(
            int number,
            float nullWeight = 0.75F)
        {
            return new Faker<AggregateModels.RangeCondition>()
                .RuleFor(
                    c => c.Start,
                    faker => faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(3))
                        .OrNull(faker, nullWeight))
                .RuleFor(
                    c => c.End,
                    (faker, current) =>
                        current?.Start != null
                            ? faker.Date.Between(current.Start.Value, DateTime.UtcNow.AddMonths(6)) as DateTime?
                            : null)
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="CalculatedTag" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="CalculatedTag" /> that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="CalculatedTag" /></returns>
        public static List<CalculatedTag> GenerateCalculatedTags(int number = 1)
        {
            return new Faker<CalculatedTag>()
                .RuleFor(t => t.Id, Guid.NewGuid().ToString())
                .RuleFor(t => t.Name, faker => faker.Commerce.Department(10))
                .RuleFor(t => t.IsInherited, faker => faker.Random.Bool(0.33f))
                .RuleFor(t => t.Type, faker => faker.Random.Enum<TagType>())
                .Generate(number);
        }
        
        /// <summary>
        ///     A method to generate fake <see cref="FunctionBasic" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="FunctionBasic" /> that should be generated</param>
        /// <returns></returns>
        public static List<LinkedFunctionObject> GenerateLinkedFunctionalObjectInstances(int number = 1)
        {
            return new Faker<LinkedFunctionObject>()
                .ApplyLinkedObjectRules()
                .RuleFor(
                    f => f.Type,
                    _ => RoleType.Function.ToString())
                .RuleFor(
                    f => f.OrganizationId,
                    faker => faker.Random.Guid().ToString())
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake functions
        /// </summary>
        /// <param name="number">the number of fake functions that should be generated</param>
        /// <returns>A list containing the generated fake functions</returns>
        public static List<FunctionBasic> GenerateFunctionBasicInstances(int number = 1)
        {
            return new Faker<FunctionBasic>()
                .ApplyFunctionBasicRules()
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake ObjectIdents
        /// </summary>
        /// <param name="number">the number of fake functions that should be generated</param>
        /// <returns>A list containing the generated fake functions</returns>
        public static List<ObjectIdent> GenerateObjectIdentInstances(int number = 1)
        {
            return new Faker<ObjectIdent>()
                .RuleFor(x => x.Id, faker => Guid.NewGuid().ToString())
                .RuleFor(
                    x => x.Type,
                    faker => (ObjectType)faker.Random.Number(
                        Enum.GetValues(typeof(ObjectType)).Cast<int>().Min()
                        + 1, // stream resolver doesn't work with ObjectType.Unknown
                        Enum.GetValues(typeof(ObjectType)).Cast<int>().Max()))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake functions
        /// </summary>
        /// <param name="number">the number of fake functions that should be generated</param>
        /// <param name="role">The related role - can be null</param>
        /// <returns>A list containing the generated fake functions</returns>
        public static List<FirstLevelProjectionFunction> GenerateFirstLevelProjectionFunctionInstances(
            int number = 1,
            FirstLevelProjectionRole role = null)
        {
            return new Faker<FirstLevelProjectionFunction>()
                .ApplyFirstLevelProjectionFunctionRules(role)
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake functions
        /// </summary>
        /// <returns>A list containing the generated fake functions</returns>
        public static FirstLevelProjectionFunction GenerateFirstLevelProjectionFunctionInstancesWithId(
            string id)
        {
            return new Faker<FirstLevelProjectionFunction>()
                .ApplyFirstLevelProjectionFunctionRules(id: id)
                .Generate(1)
                .First();
        }

        /// <summary>
        ///     A method to generate fake functions
        /// </summary>
        /// <param name="number">the number of fake roles that should be generated</param>
        /// <returns>A list containing the generated fake functions.</returns>
        public static List<FirstLevelProjectionRole> GenerateFirstLevelProjectionRoleInstances(int number = 1)
        {
            return new Faker<FirstLevelProjectionRole>()
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Source, faker => faker.PickRandom(_sources))
                .RuleFor(r => r.Name, faker => faker.Hacker.Verb())
                .RuleFor(r => r.Description, faker => faker.Lorem.Paragraph())
                .RuleFor(
                    r => r.ExternalIds,
                    (faker, role) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{role.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    r => r.Permissions,
                    faker =>
                        Enumerable
                            .Range(0, faker.Random.Int(0, 20))
                            .Select(item => faker.Hacker.Abbreviation())
                            .ToList()
                            .OrNull(faker, 0.1f))
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Random.WordsArray(0, 5).OrNull(faker, 0.1F))
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool())
                .RuleFor(r => r.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    r => r.UpdatedAt,
                    (
                        faker,
                        role) => faker.Date.Between(role.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, role)
                        => faker.Date.Between(role.CreatedAt, role.UpdatedAt)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate a fake <see cref="FunctionView" /> instance.
        /// </summary>
        public static FunctionView GenerateFunctionViewInstance(
            int minLinkedProfiles = 0,
            int maxLinkedProfiles = 3)
        {
            return new Faker<FunctionView>()
                .ApplyFunctionBasicRules()
                .RuleFor(
                    f => f.LinkedProfiles,
                    faker =>
                        GenerateMembers(
                            faker.Random.Int(minLinkedProfiles, maxLinkedProfiles),
                            ProfileKind.User,
                            ProfileKind.Group))
                .Generate(1)
                .Single();
        }

        /// <summary>
        ///     A method to generate fake <see cref="FunctionView" /> instance.
        /// </summary>
        /// <param name="number">the number of fake <see cref="FunctionView" /> that should be generated</param>
        /// <returns></returns>
        public static List<FunctionView> GenerateFunctionViewInstances(int number = 1)
        {
            return new Faker<FunctionView>()
                .ApplyFunctionBasicRules()
                .RuleFor(
                    f => f.LinkedProfiles,
                    faker => GenerateMembers(faker.Random.Int(0, 3), ProfileKind.User, ProfileKind.Group))
                .Generate(number);
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake second level projection functions.
        /// </summary>
        /// <returns>A list containing the generated functions.</returns>
        public static List<SecondLevelProjectionFunction> GenerateSecondLevelProjectionFunctions(int number = 1)
        {
            return new Faker<SecondLevelProjectionFunction>()
                .RuleFor(f => f.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(f => f.Source, faker => faker.PickRandom(_sources))
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, group) => faker.Make(
                        3,
                        () => new ExternalIdentifierAggregate(
                            $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.PickRandom(_exampleExternalIdSources))))
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
                    _ => ResolvedEventsModelFakers.NewOrganization.Generate(1).Single())
                .RuleFor(
                    f => f.OrganizationId,
                    (faker, function) => function.Organization.Id)
                .RuleFor(
                    f => f.Role,
                    _ => ResolvedEventsModelFakers.NewRole.Generate(1).Single())
                .RuleFor(f => f.RoleId, (faker, f) => f.Role.Id)
                .Generate(number);
        }

        /// <summary>
        ///     Generates one second-level-projection group with specified id and name.
        /// </summary>
        public static SecondLevelProjectionGroup GenerateSecondLevelProjectionGroup(
            string id,
            string name = null,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionGroup>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Group },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(g => g.Id, _ => id)
                .RuleFor(
                    g => g.Name,
                    faker => name ?? faker.Random.Words(3))
                .RuleFor(
                    g => g.DisplayName,
                    (_, g) => g.Name)
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    faker => faker.Random.Bool(0.1F))
                .RuleFor(
                    g => g.Weight,
                    faker => faker.Random.Double(0D, 100D))
                .Generate(1)
                .Single();
        }

        /// <summary>
        ///     Generates a list of second-level-projection group.
        /// </summary>
        public static List<SecondLevelProjectionGroup> GenerateSecondLevelProjectionGroup(
            int amount = 1,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionGroup>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Group },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(
                    g => g.DisplayName,
                    (_, g) => g.Name)
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    faker => faker.Random.Bool(0.1F))
                .RuleFor(
                    g => g.Weight,
                    faker => faker.Random.Double(0D, 100D))
                .Generate(amount);
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake first level projection users.
        /// </summary>
        /// <returns>A list containing the generated first user profiles.</returns>
        public static List<FirstLevelProjectionUser> GenerateFirstLevelProjectionUser(int number = 1)
        {
            return new Faker<FirstLevelProjectionUser>()
                .ApplyFirstLevelProfileRules()
                .RuleFor(
                    u => u.DisplayName,
                    (_, u) => u.Name)
                .RuleFor(
                    u => u.FirstName,
                    faker => faker.Name.FirstName())
                .RuleFor(
                    u => u.LastName,
                    faker => faker.Name.LastName())
                .RuleFor(
                    u => u.Name,
                    (faker, u) => $"{u.LastName},{u.FirstName}")
                .RuleFor(
                    u => u.Email,
                    (faker, u) => $"{u.FirstName}.{u.LastName}@my-mail.com")
                .RuleFor(u => u.UserName, (faker, u) => $"{u.FirstName}.{u.LastName}")
                .RuleFor(
                    u => u.UserStatus,
                    faker => "normal")
                .Generate(number);
        }

        /// <summary>
        ///     Generates a user with the given id <paramref name="id" />,
        /// </summary>
        /// <returns>A user with generated id.</returns>
        public static FirstLevelProjectionUser GenerateFirstLevelProjectionUserWithId(string id)
        {
            FirstLevelProjectionUser user = GenerateFirstLevelProjectionUser().First();
            user.Id = id;

            return user;
        }

        public static List<ProfileIdent> GenerateProfileIdent(int numberProfileIdent, ProfileKind profileKind)
        {
            // fallback
            var profileKindArray = new[] { (int)ProfileKind.Unknown };

            if (profileKind == ProfileKind.Unknown)
            {
                profileKindArray = new[] { (int)ProfileKind.Group, (int)ProfileKind.User, (int)ProfileKind.Group };
            }

            if (profileKind == ProfileKind.Group)
            {
                profileKindArray = new[] { (int)ProfileKind.Group };
            }

            if (profileKind == ProfileKind.User)
            {
                profileKindArray = new[] { (int)ProfileKind.User };
            }

            if (profileKind == ProfileKind.Organization)
            {
                profileKindArray = new[] { (int)ProfileKind.Organization };
            }

            return new Faker<ProfileIdent>()
                .RuleFor(
                    r => r.ProfileKind,
                    (faker, r) => (ProfileKind)profileKindArray[faker.Random.Number(0, profileKindArray.Length - 1)])
                .RuleFor(
                    r => r.Id,
                    faker => faker.Random.Guid().ToString())
                .Generate(numberProfileIdent);
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake first level projection users.
        /// </summary>
        /// <returns>A list containing the generated first level Roles.</returns>
        public static List<FirstLevelProjectionRole> GenerateFirstLevelRoles(int number = 1)
        {
            var roles = new[]
            {
                "Lesen",
                "Extern_Schreiben",
                "Mitarbeit",
                "Prüfer",
                "Sachbearbeitung",
                "Aktenplan",
                "Administration",
                "SGV",
                "Unfreeze"
            };

            return new Faker<FirstLevelProjectionRole>()
                .RuleFor(
                    r => r.Id,
                    r => Guid.NewGuid().ToString())
                .RuleFor(
                    r => r.Name,
                    (faker, r) => roles[faker.Random.Int(0, roles.Length - 1)])
                .RuleFor(
                    r => r.CreatedAt,
                    r => DateTime.UtcNow.ToLocalTime())
                .RuleFor(
                    r => r.UpdatedAt,
                    (_, r) => r.CreatedAt)
                .RuleFor(
                    r => r.DeniedPermissions,
                    r => r.Random.WordsArray(0, number))
                .RuleFor(
                    r => r.Permissions,
                    r => r.Random.WordsArray(0, number))
                .RuleFor(
                    r => r.ExternalIds,
                    (faker, group) => faker.Make(
                        3,
                        () => new ExternalIdentifier(
                            $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    r => r.SynchronizedAt,
                    r => DateTime.UtcNow.ToLocalTime())
                .RuleFor(
                    r => r.Description,
                    (_, r) => r.Name)
                .RuleFor(
                    r => r.IsSystem,
                    r => r.Random.Bool(0))
                .RuleFor(
                    r => r.ContainerType,
                    r => ContainerType.Role)
                .Generate(number);
        }

        public static FirstLevelProjectionRole GenerateFirstLevelRole()
        {
            return GenerateFirstLevelRoles().First();
        }

        public static List<FirstLevelProjectionTag> GenerateFirstLevelTags(int number = 1)
        {
            return new Faker<FirstLevelProjectionTag>().RuleFor(
                    t => t.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    t => t.Type,
                    t => TagType.Custom)
                .RuleFor(
                    t => t.Name,
                    t => string.Empty)
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake group basic instances.
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<GroupBasic> GenerateGroupBasicInstances(int number = 1)
        {
            return new Faker<GroupBasic>()
                .ApplyGroupBasicRules()
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake groups
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<Group> GenerateGroupInstances(int number = 1)
        {
            return new Faker<Group>()
                .ApplyGroupBasicRules()
                .RuleFor(
                    g => g.Members,
                    faker =>
                        GenerateMembers(faker.Random.Int(0, 20), ProfileKind.Group, ProfileKind.User))
                .RuleFor(g => g.MemberOf, faker => GenerateMembers(faker.Random.Int(0, 5), ProfileKind.Group))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="GroupView" />
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static IList<GroupView> GenerateGroupViewInstances(int number = 1)
        {
            return new Faker<GroupView>()
                .ApplyGroupBasicRules()
                .RuleFor(g => g.ChildrenCount, f => f.Random.Int(0, 50))
                .RuleFor(g => g.HasChildren, (_, g) => g.ChildrenCount > 0)
                .RuleFor(g => g.Tags, f => f.Random.WordsArray(0, 5))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="OrganizationView" />
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static IList<OrganizationView> GenerateOrganizationViewInstances(int number = 1)
        {
            return new Faker<OrganizationView>()
                .ApplyOrganizationBasicRules()
                .RuleFor(g => g.ChildrenCount, f => f.Random.Int(0, 50))
                .RuleFor(g => g.HasChildren, (_, g) => g.ChildrenCount > 0)
                .RuleFor(g => g.Tags, f => f.Random.WordsArray(0, 5))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="ConditionalOrganization" />
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <param name="maxRangeCondition">The maximum range conditions the organisation can contain.</param>
        /// <returns>A list containing the generated fake <see cref="ConditionalOrganization" /></returns>
        public static IList<ConditionalOrganization> GenerateOrganizationConditionalInstances(int number = 1, int maxRangeCondition = 50)
        {
            return new Faker<ConditionalOrganization>()
                .ApplyOrganizationBasicRules()
                .RuleFor(g => g.ChildrenCount, f => f.Random.Int(0, 50))
                .RuleFor(g => g.HasChildren, (_, g) => g.ChildrenCount > 0)
                .RuleFor(g => g.Tags, f => f.Random.WordsArray(0, 5))
                .RuleFor(g => g.Conditions, f => GenerateRangeConditions(f.Random.Int(0, maxRangeCondition)))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="ConditionalGroup" />
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="ConditionalGroup" /></returns>
        public static IList<ConditionalGroup> GenerateGroupConditionalInstances(int number = 1)
        {
            return new Faker<ConditionalGroup>()
                .ApplyGroupBasicRules()
                .RuleFor(g => g.ChildrenCount, f => f.Random.Int(0, 50))
                .RuleFor(g => g.HasChildren, (_, g) => g.ChildrenCount > 0)
                .RuleFor(g => g.Tags, f => f.Random.WordsArray(0, 5))
                .RuleFor(g => g.Conditions, f => GenerateRangeConditions(f.Random.Int(0, 50)))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="ConditionalUser" />
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="ConditionalUser" /></returns>
        public static IList<ConditionalUser> GenerateUserConditionalInstances(int number = 1)
        {
            return new Faker<ConditionalUser>()
                .ApplyUserBasicRules()
                .RuleFor(
                    u => u.MemberOf,
                    faker => GenerateMembers(faker.Random.Int(0, 10), ProfileKind.Group))
                .RuleFor(
                    f => f.Functions,
                    faker => GenerateLinkedFunctionalObjectInstances(faker.Random.Int(0, 50))
                        .ToList<ILinkedObject>())
                .RuleFor(g => g.Conditions, f => GenerateRangeConditions(f.Random.Int(0, 50)))
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="IProfile" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="IProfile" /> that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="IProfile" /></returns>
        public static List<ILinkedObject> GenerateLinkedObjectInstances(int number = 2)
        {
            List<ILinkedObject> minimum = GenerateLinkedFunctionalObjectInstances()
                .Cast<ILinkedObject>()
                .Concat(GenerateLinkedRoleObjectInstances())
                .ToList();

            if (number <= 2)
            {
                return minimum.Take(number).ToList();
            }

            List<ILinkedObject> extra = GenerateLinkedFunctionalObjectInstances(number - 2)
                .Cast<ILinkedObject>()
                .Concat(GenerateLinkedRoleObjectInstances(number - 2))
                .ToList();

            extra.Shuffle();
            minimum.AddRange(extra.Take(number - 2)); // always return at least one profile of any kind available

            return minimum;
        }

        /// <summary>
        ///     Generates one second-level-projection organization profile with specified name and id.
        /// </summary>
        public static SecondLevelProjectionOrganization GenerateSecondLevelProjectionOrganization(
            string id,
            string name = null,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionOrganization>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Organization },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(
                    o => o.Name,
                    faker => name ?? faker.Random.Words(3))
                .RuleFor(o => o.Id, _ => id)
                .RuleFor(
                    o => o.Name,
                    faker =>
                        faker.Company.CompanyName())
                .RuleFor(
                    o => o.DisplayName,
                    (_, o) => o.Name)
                .Generate(1)
                .Single();
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake second level projection users.
        /// </summary>
        /// <returns>A list containing the generated user profiles.</returns>
        public static List<SecondLevelProjectionOrganization> GenerateSecondLevelProjectionOrganization(
            int number = 1,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionOrganization>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Organization },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(
                    o => o.Name,
                    faker =>
                        faker.Company.CompanyName())
                .RuleFor(
                    o => o.DisplayName,
                    (_, o) => o.Name)
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake basic organizations.
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<OrganizationBasic> GenerateOrganizationBasicInstances(int number = 1)
        {
            return new Faker<OrganizationBasic>()
                .ApplyOrganizationBasicRules()
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake organizations.
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<Organization> GenerateOrganizationInstances(int number = 1)
        {
            return new Faker<Organization>()
                .ApplyOrganizationBasicRules()
                .RuleFor(o => o.CustomPropertyUrl, (f, o) => "/organizations/" + o.Id + "/customProperties")
                .RuleFor(
                    o => o.Members,
                    f => GenerateMembers(f.Random.Int(0, 5), ProfileKind.Organization))
                .RuleFor(
                    o => o.MemberOf,
                    f => GenerateMembers(f.Random.Int(0, 1), ProfileKind.Organization))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake organizations.
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<OrganizationAggregate> GenerateOrganizationAggregateInstances(int number = 1)
        {
            return new Faker<OrganizationAggregate>()
                .RuleFor(o => o.Name, faker => "Z" + faker.Random.Int(1, 50))
                .RuleFor(
                    x => x.ExternalIds,
                    (faker, organization) => faker.Make(
                        3,
                        () => new ExternalIdentifierAggregate(
                            $"{organization.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(o => o.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                .RuleFor(
                    g => g.UpdatedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime())
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    (
                        faker,
                        organization) => !organization.IsSystem && faker.Random.Bool(0.2f))
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, organization)
                        => faker.Date.Between(organization.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, faker => AggregateProfileKind.Organization)
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(g => g.TagUrl, (f, u) => "/organizations/" + u.Id + "/tagUrl")
                .RuleFor(
                    o => o.IsSubOrganization,
                    f => f.Random.Bool(0.1F))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="IProfile" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="IProfile" /> that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="IProfile" /></returns>
        public static List<IProfile> GenerateProfileInstances(int number = 3)
        {
            List<IProfile> minimum = GenerateUserBasicInstances()
                .Cast<IProfile>()
                .Concat(GenerateGroupBasicInstances())
                .Concat(GenerateOrganizationInstances())
                .ToList();

            if (number <= 3)
            {
                return minimum.Take(number).ToList();
            }

            List<IProfile> extra = GenerateUserBasicInstances(number)
                .Cast<IProfile>()
                .Concat(GenerateGroupBasicInstances(number))
                .Concat(GenerateOrganizationInstances(number))
                .ToList();

            extra.Shuffle();
            minimum.AddRange(extra.Take(number - 3)); // always return at least one profile of any kind available

            return minimum;
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake second level projection roles.
        /// </summary>
        /// <returns>A list containing the generated roles.</returns>
        public static List<SecondLevelProjectionRole> GenerateSecondLevelProjectionRoles(
            int number = 1,
            bool emptyPermissions = true,
            bool nullPermissions = true)
        {
            return new Faker<SecondLevelProjectionRole>()
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Source, faker => faker.PickRandom(_sources))
                .RuleFor(r => r.Name, faker => faker.Hacker.Verb())
                .RuleFor(r => r.Description, faker => faker.Lorem.Paragraph())
                .RuleFor(
                    r => r.ExternalIds,
                    (faker, role) => faker.Make(
                        3,
                        () => new ExternalIdentifierAggregate(
                            $"{role.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.PickRandom(_exampleExternalIdSources))))
                .RuleFor(
                    r => r.Permissions,
                    faker =>
                        Enumerable
                            .Range(0, faker.Random.Int(emptyPermissions ? 0 : 1, 20))
                            .Select(item => faker.Hacker.Abbreviation())
                            .ToList()
                            .OrNull(
                                faker,
                                nullPermissions ? 0.1f : 0.0F))
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Random.WordsArray(emptyPermissions ? 0 : 1, 5)
                        .OrNull(
                            faker,
                            nullPermissions ? 0.1F : 0.0F))
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool())
                .RuleFor(r => r.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    r => r.UpdatedAt,
                    (
                        faker,
                        role) => faker.Date.Between(role.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, role)
                        => faker.Date.Between(role.CreatedAt, role.UpdatedAt)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake roles
        /// </summary>
        /// <param name="number">the number of fake roles that should be generated</param>
        /// <returns>A list containing the generated fake roleViews</returns>
        public static List<RoleBasic> GenerateRoleBasicInstances(int number = 1)
        {
            return new Faker<RoleBasic>()
                .ApplyRoleBasicRules()
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake roles
        /// </summary>
        /// <param name="number">the number of fake roles that should be generated</param>
        /// <returns>A list containing the generated fake roleViews</returns>
        public static List<ResolvedModels.Role> GenerateRoleAggregateInstances(int number = 1)
        {
            return new Faker<ResolvedModels.Role>()
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Source, faker => faker.PickRandom(_sources))
                .RuleFor(r => r.Name, faker => faker.Hacker.Verb())
                .RuleFor(r => r.Description, faker => faker.Lorem.Paragraph())
                .RuleFor(
                    r => r.ExternalIds,
                    (faker, role) => faker.Make(
                        3,
                        () => new ExternalIdentifierAggregate(
                            $"{role.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                            faker.Database.Engine())))
                .RuleFor(
                    r => r.Permissions,
                    faker =>
                        Enumerable
                            .Range(0, faker.Random.Int(0, 20))
                            .Select(item => faker.Hacker.Abbreviation())
                            .ToList()
                            .OrNull(faker, 0.1f))
                .RuleFor(
                    r => r.DeniedPermissions,
                    faker => faker.Random.WordsArray(0, 5).OrNull(faker, 0.1F))
                .RuleFor(r => r.IsSystem, faker => faker.Random.Bool())
                .RuleFor(r => r.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    r => r.UpdatedAt,
                    (
                        faker,
                        role) => faker.Date.Between(role.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(
                    r => r.SynchronizedAt,
                    (faker, role)
                        => faker.Date.Between(role.CreatedAt, role.UpdatedAt)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake <see cref="FunctionBasic" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="FunctionBasic" /> that should be generated</param>
        /// <returns></returns>
        public static List<LinkedRoleObject> GenerateLinkedRoleObjectInstances(int number = 1)
        {
            return new Faker<LinkedRoleObject>()
                .ApplyLinkedObjectRules()
                .RuleFor(
                    f => f.Type,
                    _ => RoleType.Function.ToString())
                .Generate(number);
        }

        /// <summary>
        ///     A method to generate fake roles
        /// </summary>
        /// <param name="number">the number of fake roles that should be generated</param>
        /// <returns>A list containing the generated fake roleViews</returns>
        public static List<RoleView> GenerateRoleViewInstances(int number = 1)
        {
            return new Faker<RoleView>()
                .ApplyRoleBasicRules()
                .RuleFor(
                    r => r.LinkedProfiles,
                    faker => GenerateMembers(faker.Random.Int(0, 20), ProfileKind.Group, ProfileKind.User))
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="Tag" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="Tag" /> that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="Tag" /></returns>
        public static List<Tag> GenerateTags(int number = 1)
        {
            int index = new Random().Next(1, 20);

            return new Faker<Tag>()
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(x => x.Name, f => $"Z{index++}")
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="Tag" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="Tag" /> that should be generated</param>
        /// <returns>A list containing the generated fake <see cref="Tag" /></returns>
        public static List<AggregateModels.Tag> GenerateTagAggregateModels(int number = 1)
        {
            int index = new Random().Next(1, 20);

            return new Faker<AggregateModels.Tag>()
                .RuleFor(x => x.Name, f => $"Z{index++}")
                .RuleFor(t => t.Id, faker => faker.Random.Guid().ToString())
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="User" />
        /// </summary>
        /// <param name="number">the number of fake users that should be generated</param>
        /// <returns> A list containing the generated fake users</returns>
        public static List<UserBasic> GenerateUserBasicInstances(int number = 1)
        {
            return new Faker<UserBasic>()
                .ApplyUserBasicRules()
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="User" />
        /// </summary>
        /// <param name="number">the number of fake users that should be generated</param>
        /// <returns> A list containing the generated fake users</returns>
        public static List<User> GenerateUserInstances(int number = 1)
        {
            return new Faker<User>()
                .ApplyUserBasicRules()
                .RuleFor(x => x.CustomPropertyUrl, (f, u) => "/users/" + u.Id + "/customProperties")
                .RuleFor(
                    u => u.MemberOf,
                    faker => GenerateMembers(faker.Random.Int(0, 10), ProfileKind.Group))
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="BatchAssignmentRequest" />
        /// </summary>
        /// <param name="number">the number of fake batch assignments that should be generated</param>
        /// <param name="added">the number of fake added batch assignments that should be generated</param>
        /// <param name="removed">the number of removed added batch assignments that should be generated</param>
        /// <returns> A list containing the generated fake batch assignments</returns>
        public static List<BatchAssignmentRequest> GenerateBatchAssignments(
            int number = 1,
            int added = 1,
            int removed = 1)
        {
            return new Faker<BatchAssignmentRequest>()
                .RuleFor(x => x.Added, (f, u) => GenerateConditionalAssignments(added).ToArray())
                .RuleFor(
                    x => x.Removed,
                    (f, u) => GenerateConditionalAssignments(removed).ToArray())
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="User" />
        /// </summary>
        /// <param name="number">the number of fake users that should be generated</param>
        /// <returns> A list containing the generated fake users</returns>
        public static List<ConditionAssignment> GenerateConditionalAssignments(int number = 1)
        {
            return new Faker<ConditionAssignment>()
                .RuleFor(x => x.Id, (f, u) => f.Random.Guid().ToString())
                .RuleFor(
                    u => u.Conditions,
                    faker => GenerateRangeConditions(faker.Random.Int(0, 10)).ToArray())
                .Generate(number);
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of fake second level projection users.
        /// </summary>
        /// <returns>A list containing the generated user profiles.</returns>
        public static List<SecondLevelProjectionUser> GenerateSecondLevelProjectionUser(
            int number = 1,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionUser>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Group },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(
                    u => u.FirstName,
                    faker =>
                        $"{faker.Person.FirstName.ToLowerInvariant()}")
                .RuleFor(
                    u => u.LastName,
                    faker =>
                        $"{faker.Person.LastName.ToLowerInvariant()}")
                .RuleFor(
                    u => u.Name,
                    (faker, u) => $"{u.LastName}, {u.FirstName}")
                .Generate(number);
        }

        /// <summary>
        ///     Generates one second-level-projection user with specified id and name.
        /// </summary>
        /// <returns>A list containing the generated user profiles.</returns>
        public static SecondLevelProjectionUser GenerateSecondLevelProjectionUser(
            string id,
            string name = null,
            int minimumMemberOf = 0,
            int maximumMemberOf = 3)
        {
            return new Faker<SecondLevelProjectionUser>()
                .ApplySecondLevelProfileRules(
                    new[] { AggregateProfileKind.Group },
                    minimumMemberOf,
                    maximumMemberOf)
                .RuleFor(u => u.Id, _ => id)
                .RuleFor(
                    u => u.FirstName,
                    faker =>
                        $"{faker.Person.FirstName.ToLowerInvariant()}")
                .RuleFor(
                    u => u.LastName,
                    faker =>
                        $"{faker.Person.LastName.ToLowerInvariant()}")
                .RuleFor(
                    u => u.Name,
                    (faker, u) => name ?? $"{u.LastName}, {u.FirstName}")
                .Generate(1)
                .Single();
        }

        /// <summary>
        ///     A Method to generate fake <see cref="UserModifiableProperties" />
        /// </summary>
        /// <param name="number">the number of fake <see cref="UserModifiableProperties" /> that should be generated</param>
        /// <returns></returns>
        public static List<UserModifiableProperties> GenerateUserModifiableProperties(int number = 1)
        {
            var userStatus = new[] { "Offen", "Zugewiesen" };

            return new Faker<UserModifiableProperties>()
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.UserName, (f, u) => u.FirstName + "" + u.LastName)
                .RuleFor(x => x.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(x => x.DisplayName, (f, u) => u.FirstName + ", " + u.LastName)
                .RuleFor(x => x.UserStatus, f => f.PickRandom(userStatus))
                .Generate(number);
        }

        /// <summary>
        ///     A Method to generate fake <see cref="UserView" />
        /// </summary>
        /// <param name="number">The number of fake <see cref="UserView" />  that should be generated</param>
        /// <param name="minFunction">Minimum amount of possible function relations.</param>
        /// <param name="maxFunction">Maximum amount of possible function relations.</param>
        /// <returns></returns>
        public static List<UserView> GenerateUserViewInstances(
            int number = 1,
            int minFunction = 0,
            int maxFunction = 2)
        {
            return new Faker<UserView>()
                .ApplyUserBasicRules()
                .RuleFor(
                    u => u.MemberOf,
                    faker => GenerateMembers(faker.Random.Int(0, 10), ProfileKind.Group))
                .RuleFor(
                    f => f.Functions,
                    faker => GenerateLinkedFunctionalObjectInstances(faker.Random.Int(minFunction, maxFunction))
                        .ToList<ILinkedObject>())
                .Generate(number);
        }

        public static List<TagAssignment> GenerateTagAssignments(int numberTagAssignment, bool allIsInheritable)
        {
            return new Faker<TagAssignment>()
                .RuleFor(
                    tag => tag.TagId,
                    (faker, current) => Guid.NewGuid().ToString())
                .RuleFor(
                    tag => tag.IsInheritable,
                    (faker, current) => allIsInheritable)
                .Generate(numberTagAssignment);
        }

        public static List<AggregateModels.RangeCondition> GenerateAggregateRangeConditions(
            int number,
            float nullWeight = 0.75F)
        {
            return new Faker<AggregateModels.RangeCondition>()
                .RuleFor(
                    c => c.Start,
                    faker => faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(3))
                        .OrNull(faker, nullWeight))
                .RuleFor(
                    c => c.End,
                    (faker, current) =>
                        current?.Start != null
                            ? faker.Date.Between(current.Start.Value, DateTime.UtcNow.AddMonths(6)) as DateTime?
                            : null)
                .Generate(number);
        }

        public static AggregatedModels.EventMetaData GenerateEventData(
            string correlationId,
            AggregatedModels.EventInitiator eventInitiator,
            int versionNumber,
            string relatedEntityId)
        {
            return new Faker<AggregatedModels.EventMetaData>()
                .RuleFor(
                    meteData => meteData.CorrelationId,
                    (faker, current) => correlationId ?? Activity.Current?.Start().ToString())
                .RuleFor(
                    metaData => metaData.Initiator,
                    current => eventInitiator
                        ?? new AggregatedModels.EventInitiator
                        {
                            Type = AggregatedModels.InitiatorType.System,
                            Id = Guid.NewGuid().ToString()
                        })
                .RuleFor(
                    metaData => metaData.HasToBeInverted,
                    current => false)
                .RuleFor(
                    metaData => metaData.RelatedEntityId,
                    current => relatedEntityId ?? Guid.NewGuid().ToString())
                .RuleFor(
                    metaData => metaData.Timestamp,
                    current => DateTime.UtcNow)
                .RuleFor(
                    metaData => metaData.VersionInformation,
                    current => versionNumber == 0 ? 1 : versionNumber)
                .Generate();
        }

        /// <summary>
        ///     Generates a <paramref name="number" /> of <see cref="FirstLevelProjectionGroup" />.
        /// </summary>
        /// <returns>A list containing the generated first user profiles.</returns>
        public static List<FirstLevelProjectionGroup> GenerateFirstLevelProjectionGroup(
            int number = 1)
        {
            var sourceList = new List<string>
            {
                "api",
                "coolApi",
                "woogle"
            };

            return new Faker<FirstLevelProjectionGroup>()
                .ApplyFirstLevelProfileRules()
                .RuleFor(
                    g => g.Source,
                    (faker, g) => sourceList.PickRandom())
                .RuleFor(
                    g => g.Weight,
                    faker => faker.Random.Float(0F, 5000))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    faker => faker.Random.Bool())
                .RuleFor(
                    g => g.DisplayName,
                    faker => faker.Random.Word())
                .RuleFor(
                    g => g.Name,
                    (_, g) => g.DisplayName)
                .RuleFor(
                    u => u.IsSystem,
                    faker => faker.Random.Bool())
                .Generate(number);
        }

        public static FirstLevelProjectionGroup GenerateFirstLevelProjectionGroupWithId(string id)
        {
            FirstLevelProjectionGroup group = GenerateFirstLevelProjectionGroup().First();
            group.Id = id;

            return group;
        }

        public static FirstLevelProjectionOrganization GenerateFirstLevelProjectionOrganizationWithId(string id)
        {
            FirstLevelProjectionOrganization organization = GenerateFirstLevelProjectionOrganizationInstances().First();
            organization.Id = id;

            return organization;
        }

        public static List<ConditionObjectIdent> GenerateConditionObjectIdents(int number, ObjectType parentType)
        {
            return new Faker<ConditionObjectIdent>()
                .RuleFor(
                    ci => ci.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    ci => ci.Conditions,
                    faker => GenerateRangeConditions(faker.Random.Int(1, 20), faker.Random.Float()).ToArray())
                .RuleFor(
                    ci => ci.Type,
                    faker => ObjectTypeAssignments(ObjectType.Group))
                .Generate(number);
        }

        public static List<ObjectIdent> GenerateObjectMemberForEntity(
            int number,
            ObjectType parentType = ObjectType.Function)
        {
            return new Faker<ObjectIdent>()
                .RuleFor(
                    oi => oi.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    oi => oi.Type,
                    faker => ObjectTypeAssignments(parentType))
                .Generate(number);
        }

        public static List<IFirstLevelProjectionProfile> GenerateFirstLevelProfiles(
            int number,
            ObjectType parent = ObjectType.Function)
        {
            List<ObjectIdent> children = GenerateObjectMemberForEntity(number, parent);
            var members = new List<IFirstLevelProjectionProfile>();

            foreach (ObjectIdent child in children)
            {
                switch (child.Type)
                {
                    case ObjectType.User:
                        members.Add(GenerateFirstLevelProjectionUser().First());

                        break;
                    case ObjectType.Group:
                        members.Add(GenerateFirstLevelProjectionGroup().First());

                        break;
                    case ObjectType.Organization:
                        members.Add(GenerateFirstLevelProjectionOrganizationInstances().First());

                        break;
                    default:
                        throw new NotSupportedException($"The type {child.Type} is not supported");
                }
            }

            return members;
        }

        /// <summary>
        ///     A method to generate fake organizations.
        /// </summary>
        /// <param name="number">the number of fake groups that should be generated</param>
        /// <returns>A list containing the generated fake groups</returns>
        public static List<FirstLevelProjectionOrganization> GenerateFirstLevelProjectionOrganizationInstances(
            int number = 1)
        {
            return new Faker<FirstLevelProjectionOrganization>()
                .ApplyFirstLevelProjectionOrganizationRules()
                .Generate(number);
        }

        /// <summary>
        ///     Generates a string list of Guid.
        /// </summary>
        /// <param name="number"> The number of items that should be generated. </param>
        /// <returns> A string list of Guid</returns>
        public static IList<string> GenerateListOfGuiIds(int number = 1)
        {
            IList<string> result = new List<string>();

            for (var i = 0; i < number; i++)
            {
                result.Add(Guid.NewGuid().ToString());
            }

            return result;
        }
    }
}
