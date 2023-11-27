using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Stores;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public class DatabaseFixture : ArangoDbTestBase, IDisposable
    {
        private static List<OrganizationEntityModel> _testOrganizations;
        private static List<GroupEntityModel> _testGroups;
        private static List<UserEntityModel> _testUsers;
        private static List<FunctionObjectEntityModel> _testFunctions;
        private static List<RoleObjectEntityModel> _testRoles;

        private static readonly JsonSerializer
            _jsonSerializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);

        private readonly Lazy<Task> _preparationTask;

        public Dictionary<string, IProfile> IndexedTestProfiles { get; }

        internal static ProfileDataOptions DefaultProfileSampleData { get; } = GenerateSecurityRelatedSampleData();

        internal static List<Tag> TestTagEntities => SampleDataTestHelper.GetTestTagEntities();

        internal static List<GroupEntityModel> DefaultTestGroups => SampleDataTestHelper.GetTestGroupEntities();

        internal static List<GroupEntityModel> TestGroups =>
            _testGroups ??=
                SampleDataTestHelper.GetTestGroupEntities()
                    .Concat(
                        DefaultProfileSampleData.Profiles
                            .Where(p => p is GroupEntityModel)
                            .Cast<GroupEntityModel>())
                    .DoFunctionForEachAndReturn(g => g.ExternalIds ??= new List<ExternalIdentifier>())
                    .ToList();

        internal static List<OrganizationEntityModel> TestOrganizations =>
            _testOrganizations ??=
                SampleDataTestHelper.GetTestOrganizations()
                    .Concat(
                        DefaultProfileSampleData.Profiles
                            .OfType<OrganizationEntityModel>())
                    .DoFunctionForEachAndReturn(o => o.ExternalIds ??= new List<ExternalIdentifier>())
                    .ToList();

        internal static List<IProfileEntityModel> TestProfiles =>
            TestUsers
                .Cast<IProfileEntityModel>()
                .Concat(TestGroups)
                .Concat(TestOrganizations)
                .ToList();

        internal static List<UserEntityModel> TestUsers =>
            _testUsers ??=
                SampleDataTestHelper.GetTestUserEntities()
                    .Concat(
                        DefaultProfileSampleData.Profiles
                            .Where(p => p is UserEntityModel)
                            .Cast<UserEntityModel>())
                    .DoFunctionForEachAndReturn(u => u.ExternalIds ??= new List<ExternalIdentifier>())
                    .ToList();

        internal static List<FunctionObjectEntityModel> TestFunctions =>
            _testFunctions ??=
                SampleDataTestHelper.GetTestFunctionEntities()
                    .Concat(
                        DefaultProfileSampleData.FunctionsAndRoles
                            .Where(e => e is FunctionObjectEntityModel)
                            .Cast<FunctionObjectEntityModel>())
                    .ToList();

        internal static List<RoleObjectEntityModel> TestRoles =>
            _testRoles ??=
                SampleDataTestHelper.GetTestRoleEntities()
                    .Concat(
                        DefaultProfileSampleData.FunctionsAndRoles
                            .Where(e => e is RoleObjectEntityModel)
                            .Cast<RoleObjectEntityModel>())
                    .ToList();

        internal static List<FunctionObjectEntityModel> TestAssignmentFunctionRecursive =>
            TestFunctions.Take(3).ToList();

        internal static string TestAssignmentUserIdRecursive => _testUsers.Last()?.Id;
        
        private ModelBuilderOptions DefaultModelBuilderOptionsRead { get; }
            = DefaultModelConstellation.CreateNew(ReadTestPrefix).ModelsInfo;

        private ModelBuilderOptions DefaultModelBuilderOptionsWrite { get; }
            = DefaultModelConstellation.CreateNew(WriteTestPrefix, WriteTestQueryPrefix).ModelsInfo;

        internal ModelBuilderOptions DefaultModelBuilderOptionsEventCollectorStore { get; }
            = DefaultModelConstellation.CreateNewEventCollectorStore(EventCollectorTestPrefix).ModelsInfo;

        internal ModelBuilderOptions DefaultModelBuilderOptionsArangoSyncEntityStore { get; }
            = DefaultModelConstellation.CreateNewSyncEntityStore(ArangoSyncEntityStoreTestPrefix).ModelsInfo;

        internal ModelBuilderOptions DefaultModelBuilderOptionsAssignmentCollection { get; }
            = DefaultModelConstellation.NewAssignmentsProjectionRepository(
                                           SecondLevelAssignmentsReadPrefix,
                                           SecondLevelAssignmentReadQueryPrefix)
                                       .ModelsInfo;

        public DatabaseFixture()
        {
            _preparationTask = new Lazy<Task>(() => Task.Run(PrepareDatabaseAsync));

            IndexedTestProfiles = TestProfiles
                .ToDictionary(
                    p => p.Id,
                    p => SampleDataTestHelper.GetDefaultTestMapper().Map<IProfile>(p),
                    StringComparer.OrdinalIgnoreCase);
        }

        internal List<GroupEntityModel> GetTestGroups()
        {
            return TestGroups;
        }

        internal List<GroupEntityModel> GetDefaultTestGroups()
        {
            return DefaultTestGroups;
        }

        internal List<Tag> GetTestTags()
        {
            return TestTagEntities;
        }

        internal List<IProfileEntityModel> GetTestProfiles()
        {
            return TestProfiles;
        }

        internal List<UserEntityModel> GetTestUsers()
        {
            return TestUsers;
        }

        internal List<FunctionObjectEntityModel> GetTestFunctions()
        {
            return TestFunctions;
        }

        internal List<RoleObjectEntityModel> GetTestRoles()
        {
            return TestRoles;
        }

        internal List<FunctionObjectEntityModel> GetFunctionForAssignments()
        {
            return TestAssignmentFunctionRecursive;
        }

        private async Task PrepareDatabaseAsync()
        {
            //await GenerateAndSaveTestData("SampleData_");
            await CleanupDatabaseAsync();
            await CreateReadCollectionsAsync();
            await CreateWriteCollectionsAsync();
            await CreateTicketCollectionsAsync();
            await CreateArangoSyncEntityStoreAsync();
            await CreateEventCollectorAsync();
            await SeedReadTestDataAsync();
            await SeedSyncReadTestDataAsync();
            await CreateReadSecondLevelAssignmentsCollectionAsync();
            
            await DefaultProfileSampleData.ProjectOnArangoDbReadPartAsync(
                GetClient(),
                DefaultModelConstellation
                    .CreateNew(ReadTestPrefix)
                    .ModelsInfo,
                _jsonSerializer);

            await DefaultProfileSampleData.ProjectOnArangoDbWritePartAsync(
                GetClient(),
                DefaultModelBuilderOptionsWrite,
                _jsonSerializer);
        }

        private async Task CleanupDatabaseAsync()
        {
            IArangoDbClient client = GetClient();
            GetAllCollectionsResponse collectionInfo = await client.GetAllCollectionsAsync(true);

            if (collectionInfo.Error)
            {
                throw new Exception("Error occurred during test clean up.", collectionInfo.Exception);
            }

            foreach (CollectionEntity collectionEntity in collectionInfo.Result)
            {
                if (collectionEntity.IsSystem)
                {
                    continue;
                }

                await client.DeleteCollectionAsync(collectionEntity.Name);
            }
        }

        private static ProfileDataOptions GenerateSecurityRelatedSampleData()
        {
            IProfileDataBuilder builder = new DefaultProfileDataBuilder();

            (List<GroupEntityModel> groups, List<UserEntityModel> users, _, List<FunctionObjectEntityModel> functions,
                    List<RoleObjectEntityModel> roles, _) =
                SampleDataTestHelper.GenerateUserGroupFunctionRoleData(10, 20, 5, 5);

            builder.AddProfile(SampleDataTestHelper.GenerateTestUser(DefaultProfileSampleConstants.UserInRootGroup));
            builder.AddProfile(SampleDataTestHelper.GenerateTestUser(DefaultProfileSampleConstants.UserLonely));

            builder.AddProfile(
                SampleDataTestHelper.GenerateTestUser(DefaultProfileSampleConstants.UserInGroupOfRootGroup));

            builder.AddProfile(SampleDataTestHelper.GenerateTestGroup(DefaultProfileSampleConstants.GroupInGroup));
            builder.AddProfile(SampleDataTestHelper.GenerateTestGroup(DefaultProfileSampleConstants.RootGroup));
            builder.AddProfile(SampleDataTestHelper.GenerateTestGroup(DefaultProfileSampleConstants.RootGroupLonely));

            builder.AddRole(SampleDataTestHelper.GenerateTestRole(DefaultProfileSampleConstants.RoleOfRootGroup));
            builder.AddRole(SampleDataTestHelper.GenerateTestRole(DefaultProfileSampleConstants.RoleOfUser));

            builder.AddRelationProfileToRole(
                DefaultProfileSampleConstants.RootGroup,
                DefaultProfileSampleConstants.RoleOfRootGroup);

            builder.AddRelationProfileToRole(
                DefaultProfileSampleConstants.UserInGroupOfRootGroup,
                DefaultProfileSampleConstants.RoleOfUser);

            builder.AddRelationProfileToProfile(
                DefaultProfileSampleConstants.RootGroup,
                DefaultProfileSampleConstants.GroupInGroup);

            builder.AddRelationProfileToProfile(
                DefaultProfileSampleConstants.RootGroup,
                DefaultProfileSampleConstants.UserInRootGroup);

            builder.AddRelationProfileToProfile(
                DefaultProfileSampleConstants.GroupInGroup,
                DefaultProfileSampleConstants.UserInGroupOfRootGroup);

            builder.AddProfiles(groups.Cast<IProfileEntityModel>().Concat(users));
            builder.AddRoles(roles);
            builder.AddFunctions(functions);

            return builder.Build();
        }

        private async Task CreateReadCollectionsAsync()
        {
            IArangoDbClient client = GetClient();

            foreach (string collectionName in DefaultModelBuilderOptionsRead.GetDocumentCollections()
                         .Concat(
                             DefaultModelBuilderOptionsRead
                                 .GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in DefaultModelBuilderOptionsRead.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task CreateWriteCollectionsAsync()
        {
            IArangoDbClient client = GetClient();

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetDocumentCollections()
                         .Concat(
                             DefaultModelBuilderOptionsWrite
                                 .GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task CreateReadSecondLevelAssignmentsCollectionAsync()
        {
            IArangoDbClient client = GetClient();

            foreach (string collectionName in DefaultModelBuilderOptionsAssignmentCollection.GetDocumentCollections())
            {
                await client.CreateCollectionAsync(collectionName);
            }
        }

        private async Task CreateTicketCollectionsAsync()
        {
            IArangoDbClient client = GetClient();

            ModelBuilderOptions model = DefaultModelConstellation.CreateNewTicketStore(TicketsTestPrefix).ModelsInfo;

            foreach (string collectionName in
                     model.GetDocumentCollections().Concat(model.GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in model.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task CreateEventCollectorAsync()
        {
            IArangoDbClient client = GetClient();

            ModelBuilderOptions model = DefaultModelConstellation.CreateNewEventCollectorStore(EventCollectorTestPrefix)
                .ModelsInfo;

            foreach (string collectionName in
                     model.GetDocumentCollections().Concat(model.GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in model.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task CreateArangoSyncEntityStoreAsync()
        {
            IArangoDbClient client = GetClient();

            ModelBuilderOptions model = DefaultModelConstellation.CreateNewSyncEntityStore(ArangoSyncEntityStoreTestPrefix)
                                                                 .ModelsInfo;

            foreach (string collectionName in
                     model.GetDocumentCollections().Concat(model.GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in model.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task SeedSyncReadTestDataAsync()
        {

            (List<UserEntityModel> users, List<GroupEntityModel> groups, List<OrganizationEntityModel> organizations,
             List<FunctionObjectEntityModel> functions, List<RoleObjectEntityModel> roles, List<Tag> _)
                = SampleDataTestHelper.GetAllSampleData();

            List<UserSync> userSync = users.Select(u => GetMapper().Map<UserEntityModel, UserSync>(u)).ToList();
            List<GroupSync> groupSync = groups.Select(u => GetMapper().Map<GroupEntityModel, GroupSync>(u)).ToList();

            List<OrganizationSync> organizationSync = organizations
                .Select(u => GetMapper().Map<OrganizationEntityModel, OrganizationSync>(u))
                .ToList();

            List<FunctionSync> functionSync =
                functions.Select(f => GetMapper().Map<FunctionObjectEntityModel, FunctionSync>(f)).ToList();

            List<RoleSync> roleSync = roles.Select(f => GetMapper().Map<RoleObjectEntityModel, RoleSync>(f)).ToList();

            foreach (IEnumerable<GroupSync> group in Batch(groupSync, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsArangoSyncEntityStore.GetCollectionName<GroupSync>(),
                    group,
                    r => r.Id);
            }

            foreach (IEnumerable<UserSync> user in Batch(userSync, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsArangoSyncEntityStore.GetCollectionName<UserSync>(),
                    user,
                    r => r.Id);
            }

            foreach (IEnumerable<OrganizationSync> organization in Batch(organizationSync, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsArangoSyncEntityStore.GetCollectionName<OrganizationSync>(),
                    organization,
                    r => r.Id);
            }


            foreach (IEnumerable<RoleSync> role in Batch(roleSync, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsArangoSyncEntityStore.GetCollectionName<RoleSync>(),
                    role,
                    r => r.Id);
            }

            foreach (IEnumerable<FunctionSync> function in Batch(functionSync, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsArangoSyncEntityStore.GetCollectionName<FunctionSync>(),
                    function,
                    r => r.Id);
            }
        }

        private async Task SeedReadTestDataAsync()
        {
            (List<UserEntityModel> users, List<GroupEntityModel> groups, List<OrganizationEntityModel> organizations,
                    List<FunctionObjectEntityModel> functions, List<RoleObjectEntityModel> roles,
                    List<Tag> tags)
                = SampleDataTestHelper.GetAllSampleData();

            List<SecondLevelProjectionAssignmentsUser> userAssignment =
                SampleDataTestHelper.GetAssignmentsUsersRecursive(
                    TestAssignmentUserIdRecursive,
                    TestAssignmentFunctionRecursive.Select(func => func.Id).ToList());

            foreach (IEnumerable<UserEntityModel> user in Batch(users, 500))
            {
                await SendBatchCreateRequest(DefaultModelBuilderOptionsRead.GetQueryCollectionName<User>(), user);
            }

            foreach (IEnumerable<UserEntityModel> user in Batch(users, 500))
            {
                await SendBatchCreateRequest(DefaultModelBuilderOptionsRead.GetCollectionName<User>(), user, r => r.Id);
            }

            foreach (IEnumerable<GroupEntityModel> group in Batch(groups, 500))
            {
                await SendBatchCreateRequest(DefaultModelBuilderOptionsRead.GetQueryCollectionName<Group>(), group);
            }

            foreach (IEnumerable<GroupEntityModel> group in Batch(groups, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetCollectionName<Group>(),
                    group,
                    r => r.Id);
            }

            foreach (IEnumerable<OrganizationEntityModel> organization in Batch(organizations, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetCollectionName<Organization>(),
                    organization,
                    o => o.Id);
            }

            foreach (IEnumerable<IProfileEntityModel> profile in Batch(DefaultProfileSampleData.Profiles, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetCollectionName<IProfile>(),
                    profile,
                    r => r.Id);
            }

            foreach (IEnumerable<RoleObjectEntityModel> role in Batch(roles, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetQueryCollectionName<FunctionBasic>(),
                    role,
                    r => r.Id);
            }

            foreach (IEnumerable<FunctionObjectEntityModel> function in Batch(functions, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetQueryCollectionName<RoleBasic>(),
                    function,
                    f => f.Id);
            }

            foreach (IEnumerable<Tag> tag in Batch(tags, 500))
            {
                await SendBatchCreateRequest(DefaultModelBuilderOptionsRead.GetQueryCollectionName<Tag>(), tag, null);
            }

            foreach (IEnumerable<SecondLevelProjectionAssignmentsUser> assignmentUser in Batch(userAssignment, 500))
            {
                await SendBatchCreateRequest(
                    DefaultModelBuilderOptionsRead.GetQueryCollectionName<SecondLevelProjectionAssignmentsUser>(),
                    assignmentUser,
                    null);
            }
        }

        private Task SendBatchCreateRequest<TEntity>(
            string collectionName,
            IEnumerable<TEntity> collection)
            where TEntity : IProfile
        {
            return SendBatchCreateRequest(collectionName, collection, p => p.Id);
        }

        private async Task SendBatchCreateRequest<TEntity>(
            string collectionName,
            IEnumerable<TEntity> collection,
            Func<TEntity, string> keySelector)
        {
            HttpClient httpClient =
                GetServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient(ArangoDbClientName);

            ArangoConfiguration arangoConfig
                = GetServiceProvider().GetRequiredService<IOptionsSnapshot<ArangoConfiguration>>().Value;

            Dictionary<string, string> connectionParts =
                arangoConfig.ConnectionString.Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(
                        part => part.Split("=")[0].Trim(),
                        part => part.Split("=")[1].Trim(),
                        StringComparer.OrdinalIgnoreCase);

            var baseUri = $"{connectionParts["endpoints"]}/_db/{connectionParts["database"]}";

            HttpResponseMessage createDocs = await httpClient.SendAsync(
                new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri($"{baseUri}/_api/document/{collectionName}#multiple?overwrite=true&returnNew=true"))
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(
                            collection
                                .Select(u => u.GetJsonObjectWithInjectedKey(_jsonSerializer, keySelector))
                                .ToList(),
                            Formatting.None),
                        Encoding.UTF8,
                        "application/json"),
                    Headers =
                    {
                        Authorization = new AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(
                                Encoding.ASCII.GetBytes(
                                    $"{connectionParts["username"]}:{connectionParts["password"]}")))
                    }
                });

            if (!createDocs.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Error during test preparation. Returned code: {createDocs.StatusCode}; Message: {await createDocs.Content.ReadAsStringAsync()}");
            }
        }

        private IArangoDbClient GetClient()
        {
            var aFactory = GetServiceProvider().GetRequiredService<IArangoDbClientFactory>();

            return aFactory.Create(ArangoDbClientName);
        }

        private static async Task SaveJsonDataAsync(string filePath, object input)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await using var sWriter = new StreamWriter(fileStream);
            await using var jWriter = new JsonTextWriter(sWriter);

            JToken jObj =
                JToken.FromObject(input, _jsonSerializer);

            await jObj.WriteToAsync(jWriter);
        }

        private static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> source, int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Must be greater than zero.");
            }

            using IEnumerator<T> enumerator = source.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var i = 0;

                yield return BatchInternal();

                while (++i < size && enumerator.MoveNext())
                {
                    // discard skipped items
                }

                continue;

                // BatchInternal is a local function closing over `i` and `enumerator` that
                // executes the inner batch enumeration
                IEnumerable<T> BatchInternal()
                {
                    // ReSharper disable AccessToDisposedClosure
                    do
                    {
                        yield return enumerator.Current;
                    }
                    while (++i < size && enumerator.MoveNext());
                    // ReSharper restore AccessToDisposedClosure
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public Task PrepareAsync()
        {
            return _preparationTask.Value;
        }

        public IProfile GetTestProfile(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && IndexedTestProfiles.TryGetValue(id, out IProfile profile)
                ? profile
                : default;
        }

        public string GetArangoDbClientName()
        {
            return ArangoDbClientName;
        }

        public async Task<IReadService> GetReadServiceAsync()
        {
            await PrepareAsync();

            return GetServiceProvider().GetRequiredService<IReadService>();
        }

        public async Task<ITicketStore> GetTicketStoreAsync()
        {
            await PrepareAsync();

            return GetServiceProvider().GetRequiredService<ITicketStore>();
        }

        public async Task<IEventCollectorStore> GetEventCollectorStoreAsync()
        {
            await PrepareAsync();

            return GetServiceProvider().GetRequiredService<IEventCollectorStore>();
        }

        public async Task<IEntityStore> GetSyncRepository()
        {
            await PrepareAsync();
            return GetServiceProvider().GetRequiredService<IEntityStore>();
        }

        public async Task<IArangoDbClient> GetClientAsync()
        {
            await PrepareAsync();

            return GetClient();
        }

        public static async Task GenerateAndSaveTestData(
            string fileNamePrefix,
            string filePath = null)
        {
            (List<GroupEntityModel> groups, List<UserEntityModel> users, List<OrganizationEntityModel> organizations,
                    List<FunctionObjectEntityModel> functions, List<RoleObjectEntityModel> roles,
                    List<CustomPropertyEntityModel> customProperties)
                = SampleDataTestHelper.GenerateUserGroupFunctionRoleData();

            string storagePath = string.IsNullOrWhiteSpace(filePath) || !Directory.Exists(filePath)
                ? Directory.GetCurrentDirectory()
                : filePath;

            (List<TagTestEntity> tags, List<UserEntityModel> otherUsers, List<GroupEntityModel> otherGroups,
                List<OrganizationEntityModel> otherOus) = SampleDataTestHelper.GenerateTagsAndProfiles();

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Groups.json"),
                groups.Concat(otherGroups));

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Users.json"),
                users.Concat(otherUsers));

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Organizations.json"),
                organizations.Concat(otherOus));

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Functions.json"),
                functions);

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Roles.json"),
                roles);

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}CustomProperties.json"),
                customProperties);

            await SaveJsonDataAsync(
                Path.Combine(storagePath, $"{fileNamePrefix}Tags.json"),
                tags);
        }
    }
}
