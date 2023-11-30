using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Bogus;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Constants;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    public class SampleDataTestHelper
    {
        private static List<CalculatedTag> _profileTags;
        private static List<FunctionalAccessRightEntityModel> _functionalAccessRights;
        private static List<GroupEntityModel> _groups;
        private static List<UserEntityModel> _users;
        private static List<CustomPropertyEntityModel> _customProperties;
        private static List<Tag> _tags;
        private static List<OrganizationEntityModel> _organizations;

        internal static List<CalculatedTag> ProfileTags => _profileTags ??= GetCalculatedTagFaker().Generate(10);

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

        internal static IMapper GetDefaultTestMapper()
        {
            return new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<UserEntityModel, UserBasic>()
                            .ForMember(
                                u => u.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<GroupEntityModel, GroupBasic>()
                            .ForMember(
                                g => g.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<OrganizationEntityModel, OrganizationBasic>()
                            .ForMember(
                                g => g.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<UserEntityModel, User>()
                            .ForMember(
                                u => u.MemberOf,
                                expression => expression
                                    .MapFrom(
                                        (model, u)
                                            => u.MemberOf = model.MemberOf?
                                                .Select(
                                                    m => new Member
                                                    {
                                                        DisplayName = m.DisplayName,
                                                        Id = m.Id,
                                                        Kind = m.Kind,
                                                        Name = m.Name
                                                    })
                                                .ToList()))
                            .ForMember(
                                u => u.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<GroupEntityModel, Group>()
                            .ForMember(
                                g => g.Members,
                                expression => expression.MapFrom(
                                    (model, group, _, ctx) =>
                                        group.Members = model.Members.Select(ctx.Mapper.Map<Member>).ToList()))
                            .ForMember(
                                g => g.MemberOf,
                                expression => expression.MapFrom(
                                    (model, group, _, ctx) =>
                                        group.MemberOf = model.MemberOf.Select(ctx.Mapper.Map<Member>).ToList()))
                            .ForMember(
                                g => g.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<OrganizationEntityModel, Organization>()
                            .ForMember(
                                g => g.Members,
                                expression => expression.MapFrom(
                                    (model, group, _, ctx) =>
                                        group.Members = model.Members.Select(ctx.Mapper.Map<Member>).ToList()))
                            .ForMember(
                                g => g.MemberOf,
                                expression => expression.MapFrom(
                                    (model, group, _, ctx) =>
                                        group.MemberOf = model.MemberOf.Select(ctx.Mapper.Map<Member>).ToList()))
                            .ForMember(
                                g => g.ExternalIds,
                                expression => expression.NullSubstitute(new List<ExternalIdentifier>()));

                        cfg.CreateMap<User, UserBasic>();
                        cfg.CreateMap<Group, GroupBasic>();

                        cfg.CreateMap<IProfile, GroupBasic>()
                            .Include<GroupEntityModel, GroupBasic>()
                            .Include<Group, GroupBasic>();

                        cfg.CreateMap<GroupBasic, GroupView>();

                        cfg.CreateMap<UserBasic, UserView>()
                            .ForMember(
                                u => u.MemberOf,
                                expression => expression.NullSubstitute(new List<Member>()));

                        cfg.CreateMap<IProfile, UserBasic>()
                            .Include<UserEntityModel, UserBasic>()
                            .Include<User, UserBasic>();

                        cfg.CreateMap<FunctionObjectEntityModel, FunctionBasic>();
                        cfg.CreateMap<FunctionObjectEntityModel, FunctionView>()
                            .ForMember(f => f.Organization, setup => setup.MapFrom(
                                (model, _, __, context) => context.Mapper.Map<OrganizationBasic>(model.Organization)))
                            .ForMember(f => f.Role, setup => setup.MapFrom(
                                (model, _, __, context) => context.Mapper.Map<RoleBasic>(model.Role)));

                        cfg.CreateMap<FunctionBasic, FunctionView>();
                        cfg.CreateMap<FunctionObjectEntityModel, LinkedFunctionObject>();
                        cfg.CreateMap<RoleObjectEntityModel, RoleBasic>();
                        cfg.CreateMap<RoleObjectEntityModel, LinkedRoleObject>();
                        cfg.CreateMap<RoleObjectEntityModel, RoleView>();

                        cfg.CreateMap<UserEntityModel, Member>()
                           .ForMember(
                               u => u.Conditions,
                               expression => expression.NullSubstitute(new List<RangeCondition>()));

                        cfg.CreateMap<GroupEntityModel, Member>()
                            .ForMember(
                                g => g.Conditions,
                                expression => expression.NullSubstitute(new List<RangeCondition>()));

                        cfg.CreateMap<OrganizationEntityModel, Member>()
                            .ForMember(
                                o => o.Conditions,
                                expression => expression.NullSubstitute(new List<RangeCondition>()));

                        cfg.CreateMap<IProfileEntityModel, Member>()
                            .ForMember(
                                p => p.Conditions,
                                expression => expression.NullSubstitute(new List<RangeCondition>()))
                            .Include<UserEntityModel, Member>()
                            .Include<GroupEntityModel, Member>();

                        cfg.CreateMap<IProfile, Member>()
                            .Include<UserEntityModel, Member>()
                            .Include<GroupEntityModel, Member>()
                            .Include<OrganizationEntityModel, Member>();

                        cfg.CreateMap<IProfileEntityModel, IProfile>()
                            .Include<UserEntityModel, UserBasic>()
                            .Include<GroupEntityModel, GroupBasic>()
                            .Include<OrganizationEntityModel, OrganizationBasic>();
                        
                        cfg.CreateMap<TagTestEntity, Tag>().ReverseMap();
                        cfg.CreateMap<Tag, CalculatedTag>();
                    }));
        }

        internal static List<GroupBasic> GetTestGroups()
        {
            return LoadSampleData<GroupBasic>(WellKnownFiles.GroupsSampleFile);
        }

        internal static List<UserBasic> GetTestUsers()
        {
            return LoadSampleData<UserBasic>(WellKnownFiles.UsersSampleFile);
        }

        internal static string GetSampleDataFileName(string entityName)
        {
            return $"{WellKnownFiles.SamplesFileNamePrefix}{entityName}.json";
        }

        internal static List<UserEntityModel> GetTestUserEntities()
        {
            return _users ??= LoadSampleData<UserEntityModel>(GetSampleDataFileName("Users"));
        }

        internal static UserEntityModel GenerateTestUser(string id)
        {
            UserEntityModel user = GetUserFaker().Generate(1).First();
            user.Id = id;

            return user;
        }

        internal static GroupEntityModel GenerateTestGroup(string id)
        {
            GroupEntityModel group = GetGroupFaker().Generate(1).First();
            group.Id = id;

            return group;
        }

        internal static RoleObjectEntityModel GenerateTestRole(string id)
        {
            RoleObjectEntityModel role = GetRolesFaker().Generate(1).First();
            role.Id = id;

            return role;
        }

        internal static (List<TagTestEntity> tags, List<UserEntityModel> users, List<GroupEntityModel> groups,
            List<OrganizationEntityModel> ous)
            GenerateTagsAndProfiles()
        {
            IMapper mapper = GetDefaultTestMapper();

            List<UserEntityModel> fakeUsers = GetUserFaker(false).Generate(10);
            List<GroupEntityModel> fakeGroups = GetGroupFaker(false).Generate(5);
            List<Tag> fakeTags = GetSimpleTagFaker().Generate(50);
            List<OrganizationEntityModel> fakeOrgUnits = GetOrgFaker().Generate(2);

            fakeUsers[0].MemberOf = fakeGroups.Take(2)
                .Select(g => mapper.Map<Member>(g))
                .ToList();

            foreach (GroupEntityModel group in fakeGroups.Take(2))
            {
                group.Members = new List<Member>
                {
                    mapper.Map<Member>(fakeUsers[0])
                };
            }

            fakeGroups[0].Tags = fakeTags.Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .ToList();

            fakeUsers[0].Tags = fakeTags.Skip(10)
                .Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .Concat(
                    fakeTags.Take(2)
                        .Select(t => mapper.Map<CalculatedTag>(t))
                        .DoFunctionForEachAndReturn(t => t.IsInherited = true))
                .ToList();

            fakeGroups[3].MemberOf = new List<Member>
            {
                mapper.Map<Member>(fakeGroups[2])
            };

            fakeGroups[2].Members = new List<Member>
            {
                mapper.Map<Member>(fakeGroups[3])
            };

            fakeUsers[2].MemberOf = new List<Member>
            {
                mapper.Map<Member>(fakeGroups[3])
            };

            fakeGroups[3].Members = new List<Member>
            {
                mapper.Map<Member>(fakeUsers[2])
            };

            fakeGroups[2].Tags = fakeTags.Skip(20)
                .Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .ToList();

            fakeGroups[3].Tags = fakeTags.Skip(20)
                .Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .DoFunctionForEachAndReturn(t => t.IsInherited = true)
                .ToList();

            fakeUsers[2].Tags = fakeTags.Skip(20)
                .Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .DoFunctionForEachAndReturn(t => t.IsInherited = true)
                .ToList();

            fakeUsers[2].Tags = fakeTags.Skip(24)
                .Take(2)
                .Select(t => mapper.Map<CalculatedTag>(t))
                .ToList();

            return (fakeTags.Select(t => mapper.Map<TagTestEntity>(t)).ToList(),
                fakeUsers, fakeGroups, fakeOrgUnits);
        }

        internal static List<GroupEntityModel> GetTestGroupEntities()
        {
            return _groups ??= LoadSampleData<GroupEntityModel>(GetSampleDataFileName("Groups"));
        }
        
        
        internal static CustomPropertyEntityModel GetCustomPropertyOfProfile(
            string profileId,
            string key)
        {
            return GetTestCustomPropertyEntities()
                .FirstOrDefault(
                    cp => cp.Key == key
                        && !string.IsNullOrWhiteSpace(cp.Related)
                        && cp.Related.EndsWith($"/{profileId}"));
        }

        internal static List<CustomPropertyEntityModel> GetCustomPropertiesOfProfile(string profileId)
        {
            return GetTestCustomPropertyEntities()
                .Where(cp => !string.IsNullOrWhiteSpace(cp?.Related) && cp.Related.EndsWith($"/{profileId}"))
                .ToList();
        }

        internal static List<FunctionObjectEntityModel> GetTestFunctionEntities()
        {
            return LoadSampleData<FunctionObjectEntityModel>(GetSampleDataFileName("Functions"));
        }

        internal static List<FunctionObjectEntityModel> GetTestFunctionEntities(string prefix)
        {
            return LoadSampleData<FunctionObjectEntityModel>(GetSampleDataFileName(prefix));
        }

        internal static List<RoleObjectEntityModel> GetTestRoleEntities()
        {
            return LoadSampleData<RoleObjectEntityModel>(GetSampleDataFileName("Roles"));
        }

        internal static List<CustomPropertyEntityModel> GetTestCustomPropertyEntities()
        {
            return _customProperties ??=
                LoadSampleData<CustomPropertyEntityModel>(GetSampleDataFileName("CustomProperties"));
        }

        internal static List<OrganizationEntityModel> GetTestOrganizations()
        {
            return _organizations ??= LoadSampleData<OrganizationEntityModel>(GetSampleDataFileName("Organizations"));
        }

        internal static List<SecondLevelProjectionAssignmentsUser> GetAssignmentsUsersRecursive(
            string profileId,
            List<string> functionIds)
        {
            return GetSecondLevelProjectionAssignment(profileId, functionIds).Generate(1);
        }

        internal static List<Tag> GetTestTagEntities()
        {
            return _tags ??= LoadTagSampleData();
        }

        internal static List<Member> GetLinkProfiles(
            IList<string> includedIds)
        {
            return GetTestUserEntities()
                .Cast<IProfileEntityModel>()
                .Concat(GetTestGroupEntities())
                .Where(g => includedIds.Any(id => id.EndsWith(g.Id)))
                .Select(
                    p => new Member
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Kind = p.Kind
                    })
                .ToList();
        }

        internal static (List<UserEntityModel>, List<GroupEntityModel>, List<OrganizationEntityModel>,
            List<FunctionObjectEntityModel>, List<RoleObjectEntityModel>, List<Tag>)
            GetAllSampleData()
        {
            return (GetTestUserEntities(),
                GetTestGroupEntities(),
                GetTestOrganizations(),
                GetTestFunctionEntities(),
                GetTestRoleEntities(),
                GetTestTagEntities());
        }
        
        internal static JsonConverter[] CombineConverters(params JsonConverter[] arguments)
        {
            return arguments;
        }

        internal static JsonConverter GetLinkedObjectConverter()
        {
            return JsonSubtypesConverterBuilder.Of<ILinkedObject>(nameof(ILinkedObject.Type))
                .RegisterSubtype<LinkedFunctionObject>(RoleType.Function.ToString())
                .RegisterSubtype<LinkedRoleObject>(RoleType.Role.ToString())
                .Build();
        }

        internal static JsonConverter GetContainerProfileConverter()
        {
            return JsonSubtypesConverterBuilder
                .Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
                .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
                .Build();
        }

        internal static List<TSample> LoadSampleData<TSample>(string path)
        {
            return JsonConvert.DeserializeObject<List<TSample>>(
                File.ReadAllText(path),
                CombineConverters(
                    GetLinkedObjectConverter(),
                    GetContainerProfileConverter()));
        }

        internal static (List<GroupEntityModel> groups,
            List<UserEntityModel> users,
            List<OrganizationEntityModel> organizations,
            List<FunctionObjectEntityModel> functions,
            List<RoleObjectEntityModel> roles,
            List<CustomPropertyEntityModel> customProperties) GenerateUserGroupFunctionRoleData(
                int amountGroups = 100,
                int amountUsers = 500,
                int amountFunctions = 100,
                int amountRoles = 50,
                double percentageCustomProperties = 0.75
            )
        {
            List<GroupEntityModel> groups = GetGroupFaker().Generate(amountGroups);

            List<UserEntityModel> users = GetUserFaker().Generate(amountUsers);

            List<RoleObjectEntityModel> roles = GetRolesFaker().Generate(amountRoles);

            List<FunctionObjectEntityModel> functions = GetFunctionsFaker().Generate(amountFunctions);

            List<OrganizationEntityModel> organizations = GetOrgFaker().Generate(25);
            
            var genericFaker = new Faker();
            IMapper mapper = GetDefaultTestMapper();
            var groupParents = new Dictionary<string, List<string>>();

            BuildTree(organizations, mapper);

            functions.ForEach(
                f =>
                {
                    f.Role = mapper.Map<RoleBasic>(genericFaker.PickRandom(roles));
                    f.RoleId = f.Role?.Id;
                    f.Organization = mapper.Map<OrganizationBasic>(genericFaker.PickRandom(organizations));
                    f.OrganizationId = f.Organization?.Id;
                });

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

                    g.SecurityAssignments = GetRandomRolesAndFunctionIds(genericFaker, functions, roles);
                });

            users.ForEach(
                u =>
                {
                    u.MemberOf = groups
                        .Where(g => g.Members.Any(m => m.Id == u.Id))
                        .Select(g => mapper.Map<Member>(g))
                        .ToList();

                    u.SecurityAssignments = GetRandomRolesAndFunctionIds(genericFaker, functions, roles);
                });

            roles.ForEach(
                r =>
                {
                    r.LinkedProfiles =
                        users.Where(
                                u => u.SecurityAssignments != null
                                    && u.SecurityAssignments.Any(secAss => secAss.Id == r.Id))
                            .Select(GetDefaultTestMapper().Map<Member>)
                            .Concat(
                                groups.Where(
                                        g => g.SecurityAssignments != null
                                            && g.SecurityAssignments.Any(secAss => secAss.Id == r.Id))
                                    .Select(GetDefaultTestMapper().Map<Member>))
                            .ToList();
                });

            functions.ForEach(
                f =>
                {
                    f.LinkedProfiles =
                        users.Where(
                                u => u.SecurityAssignments != null
                                    && u.SecurityAssignments.Any(secAss => secAss.Id == f.Id))
                            .Select(GetDefaultTestMapper().Map<Member>)
                            .Concat(
                                groups.Where(
                                        g => g.SecurityAssignments != null
                                            && g.SecurityAssignments.Any(secAss => secAss.Id == f.Id))
                                    .Select(GetDefaultTestMapper().Map<Member>))
                            .ToList();
                });

            List<string> profileIds = users
                .Select(u => $"USERS/{u.Id}")
                .Concat(
                    groups
                        .Select(g => $"GROUPS/{g.Id}"))
                .ToList();

            List<string> profilesWithCustomProperties =
                PickRandom(
                        genericFaker,
                        profileIds,
                        2,
                        (int)Math.Round(profileIds.Count * percentageCustomProperties, 0))
                    .ToList();

            var customProperties = new List<CustomPropertyEntityModel>();

            foreach (string id in profilesWithCustomProperties)
            {
                customProperties.AddRange(GetCustomPropertyFaker(id).Generate(genericFaker.Random.Int(1, 5)));
            }

            return (groups, users, organizations, functions, roles, customProperties);
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

                g.MemberOf ??= new List<Member>();

                g.MemberOf.Add(mapper.Map<Member>(group));
            }
        }

        private static List<Tag> LoadTagSampleData()
        {
            IMapper mapper = GetDefaultTestMapper();

            return LoadSampleData<TagTestEntity>(GetSampleDataFileName("Tags"))
                .Select(t => mapper.Map<Tag>(t))
                .ToList();
        }

        private static List<ILinkedObject> GetRandomRolesAndFunctionIds(
            Faker genericFaker,
            List<FunctionObjectEntityModel> functions,
            List<RoleObjectEntityModel> roles)
        {
            List<FunctionObjectEntityModel> functionsOfGroup = genericFaker.PickRandom(
                    functions,
                    PickRandom(
                        genericFaker,
                        functions,
                        0,
                        100))
                .ToList();

            List<RoleObjectEntityModel> rolesOfGroup =
                roles.Where(r => functionsOfGroup.All(f => f.Role.Id != r.Id)).ToList();

            return genericFaker.PickRandom(
                    rolesOfGroup,
                    PickRandom(genericFaker, rolesOfGroup, 0, 30))
                .Select(
                    r => new LinkedRoleObject
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Type = RoleType.Role.ToString()
                    })
                .Cast<ILinkedObject>()
                .Concat(
                    functionsOfGroup
                        .Select(
                            f => new LinkedFunctionObject
                            {
                                Id = f.Id,
                                Name = f.Name,
                                Type = RoleType.Function.ToString()
                                // TODO: Problems because of model change
                                // TagFilter = f.TagFilters?.Select(t => t.Id).ToList()
                            }))
                .ToList();
        }

        private static bool CheckIfGroupIsParent(
            string parentId,
            string childId,
            Dictionary<string, List<string>> childParentRelations)
        {
            return (childParentRelations.ContainsKey(childId)
                    && childParentRelations[childId] != null
                    && childParentRelations[childId].Contains(parentId))
                || (childParentRelations.ContainsKey(parentId)
                    && childParentRelations[parentId] != null
                    && childParentRelations[parentId].Contains(childId));
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

        private static void BuildTree(
            List<OrganizationEntityModel> organizations,
            IMapper mapper)
        {
            if (organizations == null || organizations.Count < 2)
            {
                return;
            }

            mapper ??= GetDefaultMapper();

            SetupMembership(
                organizations[0],
                organizations
                    .Skip(1)
                    .Take(5),
                mapper);

            if (organizations.Count <= 7)
            {
                return;
            }

            SetupMembership(organizations[6], organizations.Skip(7), mapper);
        }

        private static void SetupMembership(
            OrganizationEntityModel parent,
            IEnumerable<OrganizationEntityModel> children,
            IMapper mapper)
        {
            List<OrganizationEntityModel> childrenList = children as List<OrganizationEntityModel>
                ?? children.ToList();

            parent.Members = childrenList.Select(mapper.Map<Member>)
                                         .ToList();

            childrenList.ForEach(
                c => c.MemberOf = new List<IContainerProfile>
                {
                    mapper.Map<OrganizationBasic>(parent)
                });
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

        private static Mapper GetDefaultMapper()
        {
            var config =
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<GroupEntityModel, GroupBasic>();
                        cfg.CreateMap<UserEntityModel, UserBasic>();
                        cfg.CreateMap<RoleObjectEntityModel, RoleBasic>();
                        cfg.CreateMap<FunctionObjectEntityModel, FunctionBasic>();
                    });

            return new Mapper(config);
        }

        private static Faker<Tag> GetSimpleTagFaker()
        {
            return new Faker<Tag>()
                .RuleFor(t => t.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(t => t.Name, faker => faker.Random.Guid().ToString())
                .RuleFor(t => t.Type, _ => TagType.Custom);
        }

        private static Faker<CustomPropertyEntityModel> GetCustomPropertyFaker(params string[] ids)
        {
            return new Faker<CustomPropertyEntityModel>()
                .RuleFor(cp => cp.Value, faker => faker.Random.Words(faker.Random.Int(1, 100)))
                .RuleFor(cp => cp.Key, faker => faker.Vehicle.Model())
                .RuleFor(cp => cp.Related, faker => faker.PickRandom(ids));
        }

        private static Faker<FunctionObjectEntityModel> GetFunctionsFaker()
        {
            return new Faker<FunctionObjectEntityModel>()
                .RuleFor(f => f.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(f => f.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    f => f.UpdatedAt,
                    (
                        faker,
                        f) => faker.Date.Between(f.CreatedAt, DateTime.UtcNow))
                .RuleFor(f => f.Name, faker => faker.Commerce.ProductName());
        }

        private static Faker<RoleObjectEntityModel> GetRolesFaker()
        {
            return new Faker<RoleObjectEntityModel>()
                .RuleFor(r => r.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(r => r.Name, faker => faker.Hacker.Verb())
                .RuleFor(r => r.Description, faker => faker.Lorem.Paragraph())
                .RuleFor(
                    r => r.Permissions,
                    faker =>
                        Enumerable
                            .Range(0, faker.Random.Int(0, 20))
                            .Select(_ => faker.Hacker.Abbreviation())
                            .ToList()
                            .OrNull(faker, 0.1f));
        }

        private static Faker<UserEntityModel> GetUserFaker(bool includeTags = true)
        {
            return new Faker<UserEntityModel>()
                .RuleFor(u => u.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    u => u.ExternalIds,
                    faker => new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(faker.Random.Guid().ToString(), "test")
                    })
                .RuleFor(u => u.CreatedAt, faker => faker.Date.Past(3).ToUniversalTime())
                .RuleFor(
                    u => u.UpdatedAt,
                    (
                        faker,
                        group) => faker.Date.Between(group.CreatedAt, DateTime.UtcNow).ToUniversalTime())
                .RuleFor(u => u.ImageUrl, faker => faker.Image.LoremFlickrUrl())
                .RuleFor(u => u.FirstName, faker => faker.Name.FirstName())
                .RuleFor(u => u.LastName, faker => faker.Name.LastName())
                .RuleFor(
                    u => u.Name,
                    (_, user)
                        => $"{user.FirstName} {user.LastName}")
                .RuleFor(
                    u => u.DisplayName,
                    (
                        _,
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
                .RuleFor(u => u.UserStatus, faker => faker.PickRandom(PossibleStatusStrings))
                .RuleFor(
                    g => g.Tags,
                    faker => includeTags
                        ? faker.PickRandom(
                                ProfileTags,
                                faker.Random.Int(0, 3))
                            .ToList()
                        : new List<CalculatedTag>())
                .RuleFor(
                    g => g.FunctionalAccessRights,
                    faker =>
                        faker.Random.Bool(0.8f)
                            ? faker.PickRandom(
                                    FunctionalAccessRights,
                                    faker.Random.Int(1, 2))
                                .ToList()
                            : new List<FunctionalAccessRightEntityModel>());
        }

        private static Faker<GroupEntityModel> GetGroupFaker(bool includeTags = true)
        {
            return new Faker<GroupEntityModel>()
                .RuleFor(g => g.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    g => g.ExternalIds,
                    (
                            faker,
                            group) =>
                        new List<ExternalIdentifier>
                        {
                            new ExternalIdentifier(
                                $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                                "test")
                        })
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
                .RuleFor(g => g.ImageUrl, faker => faker.Image.LoremFlickrUrl())
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
                        _,
                        group) => group.DisplayName.Replace(" ", ":"))
                .RuleFor(g => g.Weight, faker => faker.Random.Double(0, 500))
                .RuleFor(g => g.TagUrl, faker => faker.Internet.UrlWithPath())
                .RuleFor(
                    g => g.Tags,
                    faker => includeTags
                        ? faker.PickRandom(
                                ProfileTags,
                                faker.Random.Int(0, 3))
                            .ToList()
                        : new List<CalculatedTag>())
                .RuleFor(
                    g => g.FunctionalAccessRights,
                    faker =>
                        faker.Random.Bool(0.1f)
                            ? faker.PickRandom(
                                    FunctionalAccessRights,
                                    faker.Random.Int(1, 2))
                                .ToList()
                            : new List<FunctionalAccessRightEntityModel>());
        }

        private static Faker<OrganizationEntityModel> GetOrgFaker()
        {
            return new Faker<OrganizationEntityModel>()
                .RuleFor(g => g.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(
                    g => g.ExternalIds,
                    (
                            faker,
                            group) =>
                        new List<ExternalIdentifier>
                        {
                            new ExternalIdentifier(
                                $"{group.Id}#external{faker.Random.AlphaNumeric(faker.Random.Int(0, 50))}",
                                "test")
                        })
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
                .RuleFor(g => g.ImageUrl, faker => faker.Image.LoremFlickrUrl())
                .RuleFor(
                    g => g.SynchronizedAt,
                    (faker, group)
                        => faker.Date.Between(group.CreatedAt, DateTime.UtcNow)
                            .ToUniversalTime()
                            .OrNull(faker, 0.15f))
                .RuleFor(g => g.Kind, ProfileKind.Organization)
                .RuleFor(
                    g => g.Name,
                    (
                        _,
                        group) => group.DisplayName.Replace(" ", ":"))
                .RuleFor(g => g.TagUrl, faker => faker.Internet.UrlWithPath());
        }

        private static Faker<CalculatedTag> GetCalculatedTagFaker()
        {
            return new Faker<CalculatedTag>()
                .RuleFor(t => t.Id, faker => faker.Random.Guid().ToString())
                .RuleFor(t => t.Name, faker => faker.Commerce.Department(10))
                .RuleFor(t => t.IsInherited, faker => faker.Random.Bool(0.33f));
        }

        private static Faker<FunctionalAccessRightEntityModel> GetFunctionalAccessRightsFaker()
        {
            return new Faker<FunctionalAccessRightEntityModel>()
                .RuleFor(fa => fa.Name, faker => faker.Commerce.Color())
                .RuleFor(fa => fa.Inherited, faker => faker.Random.Bool(0.25f));
        }

        private static Faker<SecondLevelProjectionAssignmentsUser> GetSecondLevelProjectionAssignment(
            string userId,
            IReadOnlyCollection<string> functionIds)
        {
            return new Faker<SecondLevelProjectionAssignmentsUser>()
                   .RuleFor(ass => ass.ProfileId, faker => userId ?? faker.Random.Guid().ToString())
                   .RuleFor(
                       ass => ass.ActiveMemberships,
                       _ => functionIds
                                .Select(
                                    func => new ObjectIdent(
                                        func,
                                        ObjectType.Function))
                                .ToList());
        }

    }
}
