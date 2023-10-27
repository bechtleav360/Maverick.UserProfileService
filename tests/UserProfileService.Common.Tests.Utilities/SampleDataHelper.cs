using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Bogus;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities.Constants;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.Tests.Utilities.TestModels;

namespace UserProfileService.Common.Tests.Utilities
{
    /// <summary>
    ///     A class used to generate test data
    /// </summary>
    public class SampleDataHelper
    {
        private static IList<Employee> _employees;
        private static List<FunctionView> _functions;
        private static List<Group> _groups;
        private static List<RoleView> _roles;
        private static List<Tag> _tags;
        private static List<UserView> _users;

        protected static JsonSerializerSettings DefaultSerializerSettings =>
            new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters =
                {
                    new StringEnumConverter()
                }
            };

        /// <summary>
        ///     Method used to load sample data
        /// </summary>
        /// <typeparam name="TSample">Data type</typeparam>
        /// <param name="path">location of the data</param>
        /// <returns>
        ///     <see cref="List{TSample}" />
        /// </returns>
        private static List<TSample> LoadSampleData<TSample>(string path)
        {
            string absPath = Path.Combine(AppContext.BaseDirectory, path);

            return JsonConvert.DeserializeObject<List<TSample>>(File.ReadAllText(absPath));
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

        private static void AddRoleToProfile(RoleView role, IProfile profile)
        {
            if (role == null || profile == null)
            {
                return;
            }

            role.LinkedProfiles.Add(
                new Member
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    Kind = profile is Group ? ProfileKind.Group : ProfileKind.User
                });
        }

        private static void AddFunctionToProfile(FunctionView function, IProfile profile)
        {
            if (function == null || profile == null)
            {
                return;
            }

            function.LinkedProfiles.Add(
                new Member
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    Kind = profile is Group ? ProfileKind.Group : ProfileKind.User
                });
        }

        private static void AddFunctionsToProfile(List<FunctionView> functions, IProfile profile)
        {
            functions.ForEach(f => { AddFunctionToProfile(f, profile); });
        }

        private static void AddRolesToProfile(List<RoleView> roles, IProfile profile)
        {
            roles.ForEach(r => AddRoleToProfile(r, profile));
        }

        private static void ResolveMemberOf(
            Group group,
            List<Group> allGroups,
            IMapper mapper)
        {
            if (group.Members == null || !group.Members.Any())
            {
                return;
            }

            foreach (Group g in allGroups)
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

        private static void SaveJsonData(string filePath, object input)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var sWriter = new StreamWriter(fileStream);
            using var jWriter = new JsonTextWriter(sWriter);
            var jsonSerializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);
            JToken jObj = JToken.FromObject(input, jsonSerializer);
            jObj.WriteTo(jWriter);
        }

        /// <summary>
        ///     Gets a set of <see cref="Employee" />s.
        /// </summary>
        /// <returns>A list of <see cref="Employee" />s.</returns>
        public static IList<Employee> GetEmployees()
        {
            return _employees ??= LoadSampleData<Employee>(FileLocation.EmployeesSampleFile);
        }

        /// <summary>
        ///     Get all test groups
        /// </summary>
        /// <returns>
        ///     <see cref="List{Group}" />
        /// </returns>
        public static List<Group> GetTestGroups()
        {
            return _groups ??= LoadSampleData<Group>(FileLocation.GroupsSampleFile);
        }

        /// <summary>
        ///     Get all test users
        /// </summary>
        /// <returns>
        ///     <see cref="List{User}" />
        /// </returns>
        public static List<UserView> GetTestUsers()
        {
            return _users ??= LoadSampleData<UserView>(FileLocation.UsersSampleFile);
        }

        /// <summary>
        ///     Get all test roles
        /// </summary>
        /// <returns>
        ///     <see cref="List{RoleView}" />
        /// </returns>
        public static List<RoleView> GetTestRoles()
        {
            return _roles ??= LoadSampleData<RoleView>(FileLocation.RolesSampleFile);
        }

        /// <summary>
        ///     Get all test functions
        /// </summary>
        /// <returns>
        ///     <see cref="List{CustomProperty}" /></returns>
        public static List<FunctionView> GetTestFunctions()
        {
            return _functions ??= LoadSampleData<FunctionView>(FileLocation.FunctionsSampleFile);
        }

        /// <summary>
        ///     Get all test tags
        /// </summary>
        /// <returns>
        ///     <see cref="List{Tag}" /></returns>
        public static List<Tag> GetTestTags()
        {
            return _tags ??= LoadSampleData<Tag>(FileLocation.TagsSampleFile);
        }

        /// <summary>
        ///     Get all generated sample data
        /// </summary>
        /// <returns>All generated sample data</returns>
        public static (List<UserView>, List<Group>,
            List<FunctionView>, List<RoleView>,
            List<Tag>) GetAllSampleData()
        {
            return (GetTestUsers(),
                GetTestGroups(),
                GetTestFunctions(),
                GetTestRoles(),
                GetTestTags());
        }

        /// <summary>
        ///     Generate users, groups, functions, roles and tags
        /// </summary>
        /// <param name="amountGroups">amount of groups</param>
        /// <param name="amountUsers">amount of users</param>
        /// <param name="amountFunctions">amount of functions</param>
        /// <param name="amountRoles">amount of roles</param>
        /// <param name="amountTags">Amount of tags</param>
        /// <param name="writeData">Should the data be written ot a file?</param>
        /// <returns> generated groups, users, functions, roles, secOs and tags</returns>
        public static (List<Group> groups, List<UserView> users, List<FunctionView> functions, List<RoleView> roles,
            List<Tag> tags) GenerateSampleData(
                int amountGroups = 100,
                int amountUsers = 500,
                int amountFunctions = 100,
                int amountRoles = 50,
                int amountTags = 5,
                bool writeData = false
            )
        {
            List<Group> groups = MockDataGenerator.GenerateGroupInstances(amountGroups);
            List<UserView> users = MockDataGenerator.GenerateUserViewInstances(amountUsers);
            List<RoleView> roles = MockDataGenerator.GenerateRoleViewInstances(amountRoles);
            List<FunctionView> functions = MockDataGenerator.GenerateFunctionViewInstances(amountFunctions);
            List<Tag> tags = MockDataGenerator.GenerateTags(amountTags);

            var genericFaker = new Faker();
            Mapper mapper = GetDefaultMapper();

            var groupParents = new Dictionary<string, List<string>>();

            functions.ForEach(f => { f.Role = mapper.Map<RoleBasic>(genericFaker.PickRandom(roles)); });

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
                                    .Map<GroupBasic>(ge))
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
                            .Select(u => mapper.Map<UserBasic>(u))
                            .Select(u => mapper.Map<Member>(u)));

                    g.Members = members;
                    ResolveMemberOf(g, groups, mapper);
                    AddRolesToProfile(PickRandom(genericFaker, roles, 0, 50).ToList(), g);
                    AddFunctionsToProfile(PickRandom(genericFaker, functions, 0, 100).ToList(), g);
                });

            users.ForEach(
                u =>
                {
                    u.MemberOf = groups
                        .Where(g => g.Members.Any(m => m.Id == u.Id))
                        .Select(g => mapper.Map<Member>(g))
                        .ToList();

                    AddRolesToProfile(PickRandom(genericFaker, roles, 0, 50).ToList(), u);
                    IEnumerable<FunctionView> func = PickRandom(genericFaker, functions, 0, 100);
                    List<FunctionView> functionList = func.ToList();
                    AddFunctionsToProfile(functionList, u);

                    functionList
                        .ForEach(
                            f => { (u.Functions ??= new List<ILinkedObject>()).Add(mapper.Map<ILinkedObject>(f)); });
                });

            if (writeData)
            {
                SaveJsonData(FileLocation.GroupsSampleFile, groups);
                SaveJsonData(FileLocation.UsersSampleFile, users);
                SaveJsonData(FileLocation.FunctionsSampleFile, functions);
                SaveJsonData(FileLocation.RolesSampleFile, roles);
                SaveJsonData(FileLocation.TagsSampleFile, tags);
            }

            return (groups, users, functions, roles, tags);
        }

        /// <summary>
        ///     Get default mapper to convert generated object to usual model
        /// </summary>
        /// <returns>Mapper <see cref="Mapper" /></returns>
        public static Mapper GetDefaultMapper()
        {
            var config =
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<Group, GroupBasic>();
                        cfg.CreateMap<User, UserBasic>();
                        cfg.CreateMap<UserView, UserBasic>();
                        cfg.CreateMap<RoleView, RoleBasic>();
                        cfg.CreateMap<FunctionView, FunctionBasic>();

                        cfg.CreateMap<GroupBasic, Member>()
                            .ForMember(g => g.DisplayName, m => { m.MapFrom(s => s.DisplayName); })
                            .ForMember(g => g.Id, m => { m.MapFrom(s => s.Id); })
                            .ForMember(g => g.Kind, m => { m.MapFrom(s => s.Kind); })
                            .ForMember(g => g.Name, m => { m.MapFrom(s => s.Name); });

                        cfg.CreateMap<UserBasic, Member>()
                            .ForMember(g => g.DisplayName, m => { m.MapFrom(s => s.DisplayName); })
                            .ForMember(g => g.Id, m => { m.MapFrom(s => s.Id); })
                            .ForMember(g => g.Kind, m => { m.MapFrom(s => s.Kind); })
                            .ForMember(g => g.Name, m => { m.MapFrom(s => s.Name); });

                        cfg.CreateMap<FunctionView, ILinkedObject>()
                            .ForMember(linkObj => linkObj.Id, m => { m.MapFrom(func => func.Id); })
                            .ForMember(linkObj => linkObj.Name, m => { m.MapFrom(func => func.Name); })
                            .ForMember(linkObj => linkObj.Type, m => { m.MapFrom(func => func.Type.ToString()); });
                    });

            return new Mapper(config);
        }
    }
}
