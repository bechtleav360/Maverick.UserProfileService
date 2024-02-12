using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bogus;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.UnitTests.V2.Constants;
using UserProfileService.Common.Tests.Utilities.Extensions;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    internal class SampleDataTestHelper
    {
        private static List<CalculatedTag> _profileTags;
        private static List<FunctionalAccessRightEntityModel> _functionalAccessRights;
        private static List<GroupEntityModel> _groups;

        internal static List<CalculatedTag> ProfileTags => _profileTags ??= GetTagFaker().Generate(10);

        internal static List<FunctionalAccessRightEntityModel> FunctionalAccessRights =>
            _functionalAccessRights ??= GetFunctionalAccessRightsFaker().Generate(15);

        internal static List<string> PossibleStatusStrings =>
            new List<string>
            {
                "active",
                "not active",
                "on vacation",
                "sick"
            };

        internal static List<UserEntityModel> GetTestUserEntities()
        {
            return LoadSampleData<UserEntityModel>(
                WellKnownFiles.UsersGroupsSampleFile,
                "Users");
        }

        internal static List<GroupEntityModel> GetTestGroupEntities()
        {
            return _groups ??= LoadSampleData<GroupEntityModel>(
                WellKnownFiles.UsersGroupsSampleFile,
                "Groups",
                JsonHelpers.GetContainerProfileConverter());
        }

        internal static Task<(List<UserEntityModel>, List<GroupEntityModel>)> GetTestUserAndGroupEntities()
        {
            return LoadSampleDataAsync<UserEntityModel, GroupEntityModel>(
                WellKnownFiles.UsersGroupsSampleFile,
                "Users",
                "Groups");
        }

        internal static List<TSample> LoadSampleData<TSample>(string path, params JsonConverter[] converters)
        {
            return JsonConvert.DeserializeObject<List<TSample>>(File.ReadAllText(path));
        }

        internal static async Task<(List<TFirst>, List<TSecond>)> LoadSampleDataAsync<TFirst, TSecond>(
            string path,
            string jsonPath,
            string jsonPathSecond,
            params JsonConverter[] converters)
        {
            using var sr = new StreamReader(path);
            using var jReader = new JsonTextReader(sr);
            JToken jObj = await JToken.ReadFromAsync(jReader);

            if (jObj[jsonPath]?.Type != JTokenType.Array)
            {
                throw new Exception(
                    $"Wrong json date format in the first sample data set. Expecting ARRAY in path {jsonPath} of file {path}.");
            }

            if (jObj[jsonPathSecond]?.Type != JTokenType.Array)
            {
                throw new Exception(
                    $"Wrong json date format in the second sample data set. Expecting ARRAY in path {jsonPathSecond} of file {path}.");
            }

            var jsonSerializer = JsonSerializer.CreateDefault(
                new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    Converters = converters
                });

            return (jObj[jsonPath].ToObject<List<TFirst>>(jsonSerializer),
                jObj[jsonPathSecond].ToObject<List<TSecond>>(jsonSerializer));
        }

        internal static List<TSample> LoadSampleData<TSample>(
            string path,
            string jsonPath,
            params JsonConverter[] converters)
        {
            using var sr = new StreamReader(path);
            using var jReader = new JsonTextReader(sr);
            JToken jObj = JToken.ReadFrom(jReader);

            if (jObj[jsonPath]?.Type != JTokenType.Array)
            {
                throw new Exception(
                    $"Wrong json date format in the sample data set. Expecting ARRAY in path {jsonPath} of file {path}.");
            }

            var jsonSerializer = JsonSerializer.CreateDefault(
                new JsonSerializerSettings
                {
                    Converters = converters
                });

            return jObj[jsonPath].ToObject<List<TSample>>(jsonSerializer);
        }

        internal static UserEntityModel GetTestUserEntity(string id)
        {
            return GetTestUserEntities().FirstOrDefault(u => u.Id == id);
        }

        internal static GroupEntityModel GetTestGroupEntity(string id)
        {
            return GetTestGroupEntities().FirstOrDefault(g => g.Id == id);
        }

        internal static List<IProfile> GenerateProfileDataUnrelated(
            int minUsers,
            int maxUsers,
            int minGroups,
            int maxGroups)
        {
            var faker = new Faker();

            List<UserEntityModel> users = GetUserFaker(false).Generate(faker.Random.Int(minUsers, maxUsers));

            List<GroupEntityModel> groups =
                GetGroupFaker(false, false).Generate(faker.Random.Int(minGroups, maxGroups));

            return users.Cast<IProfile>()
                .Concat(groups)
                .ToList();
        }

        internal static (List<GroupEntityModel> groups, List<UserEntityModel> users) GenerateUserAndGroupData()
        {
            List<GroupEntityModel> groups = GetGroupFaker(false, false).Generate(100);

            List<UserEntityModel> users = GetUserFaker(false).Generate(500);

            var genericFaker = new Faker();
            Mapper mapper = GetDefaultMapper();
            var groupParents = new Dictionary<string, List<string>>();

            groups.ForEach(
                g =>
                {
                    List<Member> members = genericFaker.Random.Bool(0.2f)
                        ? PickRandom(
                                genericFaker,
                                groups.Where(ge => ge.Id != g.Id && !CheckIfGroupIsParent(g.Id, ge.Id, groupParents)),
                                1,
                                10)
                            .Select(
                                ge => mapper
                                    .Map<Member>(ge))
                            .ToList()
                        : new List<Member>();

                    AddGroupParentRelations(g.Id, members, groupParents);

                    if (!genericFaker.Random.Bool(0.8f))
                    {
                        g.Members = members;
                        ResolveMemberOf(g, groups, mapper);

                        return;
                    }

                    members.AddRange(
                        PickRandom(genericFaker, users, 1, 50)
                            .Select(u => mapper.Map<Member>(u)));

                    g.Members = members;
                    ResolveMemberOf(g, groups, mapper);
                });

            users.ForEach(
                u =>
                {
                    u.MemberOf = groups
                        .Where(g => g.Members.Any(m => m.Id == u.Id))
                        .Select(g => mapper.Map<Member>(g))
                        .ToList();
                });

            return (groups, users);
        }

        internal static void ResolveMemberOf(
            GroupEntityModel group,
            List<GroupEntityModel> allGroups,
            IMapper mapper)
        {
            if (group.Members == null || !group.Members.Any())
            {
                return;
            }

            foreach (GroupEntityModel g in allGroups)
            {
                if ((g.MemberOf != null && g.MemberOf.Any(m => m.Id == group.Id))
                    || group.Members.All(m => m.Id != g.Id))
                {
                    continue;
                }

                if (g.MemberOf == null)
                {
                    g.MemberOf = new List<Member>();
                }

                g.MemberOf.Add(mapper.Map<Member>(group));
            }
        }

        internal static Faker<UserEntityModel> GetUserFaker(
            bool includeMemberOf,
            int minMemberOf = 0,
            int maxMemberOf = 10)
        {
            Faker<UserEntityModel> newFaker = new Faker<UserEntityModel>()
                .RuleFor(u => u.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    u => u.ExternalIds,
                    faker => faker.System.AndroidId()
                        .CreateSimpleExternalIdentifiers())
                .RuleFor(u => u.CreatedAt, faker => faker.Date.Past(3))
                .RuleFor(
                    u => u.UpdatedAt,
                    (
                        faker,
                        group) => faker.Date.Between(group.CreatedAt, DateTime.UtcNow))
                .RuleFor(u => u.ImageUrl, faker => faker.Image.LoremFlickrUrl())
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
                            .OrNull(faker, 0.15f))
                .RuleFor(
                    u => u.Email,
                    (faker, user)
                        => faker.Internet.Email(user.FirstName, user.LastName))
                .RuleFor(u => u.Kind, ProfileKind.User)
                .RuleFor(u => u.UserStatus, faker => faker.PickRandom(PossibleStatusStrings))
                .RuleFor(
                    g => g.Tags,
                    faker => faker.PickRandom(
                            ProfileTags,
                            faker.Random.Int(0, 3))
                        .ToList())
                .RuleFor(
                    g => g.FunctionalAccessRights,
                    faker =>
                        faker.Random.Bool(0.8f)
                            ? faker.PickRandom(
                                    FunctionalAccessRights,
                                    faker.Random.Int(1, 2))
                                .ToList()
                            : new List<FunctionalAccessRightEntityModel>())
                .RuleFor(g => g.CustomPropertyUrl, faker => faker.Internet.Url())
                .RuleFor(g => g.TagUrl, faker => faker.Internet.Url());

            if (includeMemberOf)
            {
                newFaker
                    .RuleFor(
                        u => u.MemberOf,
                        faker =>
                            GetGroupFaker(false, false)
                                .Generate(faker.Random.Int(minMemberOf, maxMemberOf))
                                .Select(GetDefaultMapper().Map<Member>)
                                .ToList());
            }

            return newFaker;
        }

        internal static IList<GroupEntityModel> GetFakeGroups(
            int amount,
            Action<Faker, GroupEntityModel> modifier)
        {
            var faker = new Faker();

            List<GroupEntityModel> groups = GetGroupFaker(false, false)
                .Generate(amount)
                .ToList();

            groups.ForEach(g => modifier.Invoke(faker, g));

            return groups;
        }

        internal static Faker<GroupEntityModel> GetGroupFaker(
            bool includeMembers,
            bool includeMemberOf,
            int minGroupMembers = 0,
            int maxGroupMembers = 5,
            int minUserMembers = 0,
            int maxUserMembers = 10)
        {
            Faker<GroupEntityModel> newFaker = new Faker<GroupEntityModel>()
                .RuleFor(g => g.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    g => g.ExternalIds,
                    (
                            faker,
                            group) =>
                        $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}"
                            .CreateSimpleExternalIdentifiers())
                .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3))
                .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                .RuleFor(
                    g => g.UpdatedAt,
                    (
                        faker,
                        group) => faker.Date.Between(group.CreatedAt, DateTime.UtcNow))
                .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                .RuleFor(
                    g => g.IsMarkedForDeletion,
                    (
                        faker,
                        group) => !group.IsSystem && faker.Random.Bool(0.2f))
                .RuleFor(g => g.ImageUrl, faker => faker.Image.LoremFlickrUrl())
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, ProfileKind.Group)
                .RuleFor(
                    g => g.Name,
                    (
                        faker,
                        group) => group.DisplayName.Replace(" ", ":"))
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(g => g.TagUrl, faker => faker.Internet.UrlWithPath())
                .RuleFor(
                    g => g.Tags,
                    faker => faker.PickRandom(
                            ProfileTags,
                            faker.Random.Int(0, 3))
                        .ToList())
                .RuleFor(
                    g => g.FunctionalAccessRights,
                    faker =>
                        faker.Random.Bool(0.1f)
                            ? faker.PickRandom(
                                    FunctionalAccessRights,
                                    faker.Random.Int(1, 2))
                                .ToList()
                            : new List<FunctionalAccessRightEntityModel>())
                .RuleFor(g => g.TagUrl, faker => faker.Internet.Url());

            if (includeMembers)
            {
                newFaker
                    .RuleFor(
                        g => g.Members,
                        faker =>
                            GetGroupFaker(false, false)
                                .Generate(faker.Random.Int(minGroupMembers, maxGroupMembers))
                                .Select(GetDefaultMapper().Map<Member>)
                                .Concat(
                                    GetUserFaker(false)
                                        .Generate(faker.Random.Int(minUserMembers, maxUserMembers))
                                        .Select(GetDefaultMapper().Map<Member>))
                                .ToList());
            }

            if (includeMemberOf)
            {
                newFaker
                    .RuleFor(
                        g => g.MemberOf,
                        faker =>
                            GetGroupFaker(false, false)
                                .Generate(faker.Random.Int(0, 5))
                                .Select(GetDefaultMapper().Map<Member>)
                                .ToList());
            }

            return newFaker;
        }

        internal static Faker<OrganizationEntityModel> GetOrganizationFaker(
            bool includeMembers,
            bool includeMemberOf,
            int minOrgMembers = 0,
            int maxOrgMembers = 5)
        {
            Faker<OrganizationEntityModel> newFaker =
                new Faker<OrganizationEntityModel>()
                    .RuleFor(g => g.Id, faker => faker.Random.Guid().ToString())
                    .RuleFor(
                        g => g.ExternalIds,
                        (
                                faker,
                                group) =>
                            $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}"
                                .CreateSimpleExternalIdentifiers())
                    .RuleFor(g => g.CreatedAt, faker => faker.Date.Past(3))
                    .RuleFor(g => g.DisplayName, faker => faker.Random.Words(faker.Random.Int(1, 3)))
                    .RuleFor(
                        g => g.UpdatedAt,
                        (
                            faker,
                            group) => faker.Date.Between(group.CreatedAt, DateTime.UtcNow))
                    .RuleFor(g => g.IsSystem, faker => faker.Random.Bool(0.3f))
                    .RuleFor(
                        g => g.IsMarkedForDeletion,
                        (
                            faker,
                            group) => !group.IsSystem && faker.Random.Bool(0.2f))
                    .RuleFor(g => g.ImageUrl, faker => faker.Image.LoremFlickrUrl())
                    .RuleFor(
                        g => g.SynchronizedAt,
                        (faker, group)
                            => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                                .OrNull(faker, 0.15f))
                    .RuleFor(g => g.Kind, ProfileKind.Organization)
                    .RuleFor(
                        g => g.Name,
                        (
                            faker,
                            group) => group.DisplayName.Replace(" ", ":"))
                    .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                    .RuleFor(g => g.TagUrl, faker => faker.Internet.UrlWithPath())
                    .RuleFor(
                        g => g.Tags,
                        faker => faker.PickRandom(
                                ProfileTags,
                                faker.Random.Int(0, 3))
                            .ToList())
                    .RuleFor(
                        g => g.FunctionalAccessRights,
                        faker =>
                            faker.Random.Bool(0.1f)
                                ? faker.PickRandom(
                                        FunctionalAccessRights,
                                        faker.Random.Int(1, 2))
                                    .ToList()
                                : new List<FunctionalAccessRightEntityModel>())
                    .RuleFor(g => g.CustomPropertyUrl, faker => faker.Internet.Url())
                    .RuleFor(g => g.TagUrl, faker => faker.Internet.Url());

            if (includeMembers)
            {
                newFaker
                    .RuleFor(
                        g => g.Members,
                        faker =>
                            GetOrganizationFaker(false, false)
                                .Generate(faker.Random.Int(minOrgMembers, maxOrgMembers))
                                .Select(
                                    p => new Member
                                         {
                                             Name = p.Name,
                                             Kind = p.Kind,
                                             Conditions = p.Conditions,
                                             DisplayName = p.DisplayName,
                                             Id = p.Id,
                                             IsActive = p.Conditions?.Any(c => c.IsActive()) ?? true,
                                             ExternalIds = p.ExternalIds
                                         })
                                .ToList());
            }

            if (includeMemberOf)
            {
                newFaker
                    .RuleFor(
                        g => g.MemberOf,
                        faker =>
                            GetOrganizationFaker(false, false)
                                .Generate(faker.Random.Int(0, 5))
                                .Select(GetDefaultMapper().Map<OrganizationBasic>)
                                .ToList<IContainerProfile>());
            }

            return newFaker;
        }

        internal static IList<GroupEntityModel> GetGroupsWithCorrectAndInvalidTags(string correctTag)
        {
            IList<GroupEntityModel> wrongTaggedGroups =
                GetFakeGroups(
                    2,
                    (faker, g) => g.Tags = new List<CalculatedTag>
                    {
                        new CalculatedTag
                        {
                            Name = $"NOT_{correctTag}{faker.Random.AlphaNumeric(10)}"
                        }
                    });

            IList<GroupEntityModel> correctTaggedGroups =
                GetFakeGroups(
                    4,
                    (_, g) => g.Tags = new List<CalculatedTag>
                    {
                        new CalculatedTag
                        {
                            Name = correctTag
                        }
                    });

            List<GroupEntityModel> parents =
                correctTaggedGroups.Take(3).ToList();

            parents[0].Members = new List<GroupEntityModel>
                {
                    wrongTaggedGroups[0],
                    correctTaggedGroups[3]
                }
                .Select(GetDefaultMapper().Map<Member>)
                .ToList();

            correctTaggedGroups[3].MemberOf = new List<Member>
            {
                GetDefaultMapper().Map<Member>(parents[0])
            };

            wrongTaggedGroups[0].MemberOf = new List<Member>
            {
                GetDefaultMapper().Map<Member>(parents[0])
            };

            parents[1].Members = new List<GroupEntityModel>
                {
                    wrongTaggedGroups[1]
                }
                .Select(GetDefaultMapper().Map<Member>)
                .ToList();

            wrongTaggedGroups[1].MemberOf = new List<Member>
            {
                GetDefaultMapper().Map<Member>(parents[1])
            };

            return parents;
        }

        internal static FunctionObjectEntityModel GetSampleFunctionEntityModel(string funcId)
        {
            var roleFaker = new Faker<RoleBasic>();

            roleFaker
                .RuleFor(r => r.Name, _ => "READ")
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Description, _ => "Will include all read permissions.")
                .RuleFor(r => r.IsSystem, _ => false)
                .RuleFor(
                    r => r.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(faker.Random.AlphaNumeric(10), "faker")
                    })
                .RuleFor(r => r.Permissions, _ => new List<string>())
                .RuleFor(r => r.DeniedPermissions, _ => new List<string>())
                .RuleFor(r => r.CreatedAt, faker => faker.Date.Past(1, DateTime.UtcNow.AddMonths(-5)))
                .RuleFor(r => r.UpdatedAt, (faker, r) => faker.Date.Between(r.CreatedAt, DateTime.UtcNow.AddMonths(-1)))
                .RuleFor(r => r.Source, _ => "faker")
                .RuleFor(r => r.SynchronizedAt, (faker, r) => faker.Date.Soon(1, r.UpdatedAt));

            var tagFilterFaker = new Faker<Tag>();

            tagFilterFaker
                .RuleFor(t => t.Name, _ = "Testteam")
                .RuleFor(t => t.Type, _ => TagType.Custom)
                .RuleFor(t => t.Id, faker => faker.Random.Guid().ToString());

            var profilesFaker = new Faker<Member>();

            profilesFaker
                .RuleFor(p => p.Name, faker => faker.Name.FullName())
                .RuleFor(p => p.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(p => p.ExternalIds, _ => null)
                .RuleFor(p => p.Kind, faker => faker.PickRandom<ProfileKind>());

            List<Member> profiles = profilesFaker.Generate(3);

            var funcFaker = new Faker<FunctionObjectEntityModel>();

            funcFaker
                .RuleFor(f => f.Name, _ => "Testteam READ")
                .RuleFor(f => f.Id, _ => funcId)
                .RuleFor(f => f.ExternalIds, _ => new List<ExternalIdentifier>())
                .RuleFor(f => f.Role, _ => roleFaker.Generate(1).Single())
                .RuleFor(f => f.RoleId, (_, f) => f.Role.Id)
                .RuleFor(
                    f => f.CreatedAt,
                    (faker, f)
                        => faker.Date.Between(f.Role.CreatedAt, DateTime.UtcNow.AddDays(-2)))
                .RuleFor(
                    f => f.UpdatedAt,
                    (faker, f)
                        => faker.Date.Between(f.CreatedAt, DateTime.UtcNow))
                .RuleFor(f => f.Source, _ => "Api")
                .RuleFor(f => f.LinkedProfiles, _ => profiles.Select(GetDefaultMapper().Map<Member>).ToList());

            return funcFaker.Generate(1).Single();
        }

        private static bool CheckIfGroupIsParent(
            string parentId,
            string childId,
            Dictionary<string, List<string>> childParentRelations)
        {
            return childParentRelations.ContainsKey(childId)
                && childParentRelations[childId] != null
                && childParentRelations[childId].Contains(parentId);
        }

        private static void AddGroupParentRelations(
            string parentId,
            List<Member> children,
            Dictionary<string, List<string>> childParentRelations)
        {
            if (children == null || children.Count == 0)
            {
                return;
            }

            foreach (Member child in children)
            {
                if (!childParentRelations.ContainsKey(child.Id))
                {
                    childParentRelations.Add(child.Id, new List<string>());
                }

                if (!childParentRelations[child.Id].Contains(parentId))
                {
                    childParentRelations[child.Id].Add(parentId);
                }
            }
        }

        private static IEnumerable<TElem> PickRandom<TElem>(
            Faker faker,
            IEnumerable<TElem> elements,
            int min,
            int max)
        {
            if (!(elements is List<TElem> collection))
            {
                collection = elements.ToList();
            }

            if (collection.Count == 0)
            {
                return Enumerable.Empty<TElem>();
            }

            if (collection.Count == 1)
            {
                return collection;
            }

            return faker.PickRandom(
                collection,
                faker.Random.Int(
                    Math.Min(min, collection.Count),
                    Math.Min(max, collection.Count)));
        }

        private static Faker<CalculatedTag> GetTagFaker()
        {
            return new Faker<CalculatedTag>()
                .RuleFor(t => t.Name, faker => faker.Commerce.Department(10))
                .RuleFor(t => t.IsInherited, faker => faker.Random.Bool(0.33f));
        }

        private static Faker<FunctionalAccessRightEntityModel> GetFunctionalAccessRightsFaker()
        {
            return new Faker<FunctionalAccessRightEntityModel>()
                .RuleFor(fa => fa.Name, faker => faker.Commerce.Color())
                .RuleFor(fa => fa.Inherited, faker => faker.Random.Bool(0.25f));
        }

        public static Mapper GetDefaultMapper()
        {
            var config =
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<GroupEntityModel, GroupBasic>();
                        cfg.CreateMap<UserEntityModel, UserBasic>();
                        cfg.CreateMap<UserEntityModel, User>();
                        cfg.CreateMap<GroupEntityModel, Group>();
                        cfg.CreateMap<OrganizationEntityModel, OrganizationBasic>();
                        cfg.CreateMap<OrganizationEntityModel, Organization>();
                        cfg.CreateMap<GroupEntityModel, Member>();
                        cfg.CreateMap<UserEntityModel, Member>();
                        cfg.CreateMap<OrganizationEntityModel, Member>();
                        cfg.CreateMap<FunctionObjectEntityModel, FunctionView>();
                        cfg.CreateMap<RoleObjectEntityModel, RoleView>();
                    });

            return new Mapper(config);
        }
    }
}
