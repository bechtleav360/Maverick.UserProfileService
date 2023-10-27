using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding
{
    internal class FirstLevelProjectionSeedingService
    {
        private readonly IArangoDbClient _client;
        private readonly ModelBuilderOptions _modelBuilderOptions;
        private readonly ModelBuilderOptions _modelBuilderOptionsRead;

        internal FirstLevelProjectionSeedingService(
            IArangoDbClient client,
            ModelBuilderOptions modelOptions,
            ModelBuilderOptions modelOptionsRead)
        {
            _client = client;
            _modelBuilderOptions = modelOptions;
            _modelBuilderOptionsRead = modelOptionsRead;
        }

        private async Task CreateEntities(
            IList<object> entities,
            Type entityType,
            string keyName,
            string revision,
            TestType testScope = TestType.Undefined)
        {
            ModelBuilderOptions usedModelBuilderOptions = testScope == TestType.ReadTest
                ? _modelBuilderOptionsRead
                : _modelBuilderOptions;

            string collection = usedModelBuilderOptions.GetCollectionName(entityType);

            await _client.CreateDocumentsAsync(
                collection,
                entities.Select(
                        e =>
                        {
                            JObject json = null;

                            bool success = e != null
                                && e.GetType() == entityType
                                && TryGetJsonObjectWithInjectedKey(
                                    e,
                                    keyName,
                                    revision,
                                    out json);

                            return new
                            {
                                success,
                                json
                            };
                        })
                    .Where(e => e.success)
                    .Select(e => e.json)
                    .ToList());
        }

        private async Task CreateEntities<T>(
            IList<ExtendedEntity<T>> entities,
            Func<T, string> keyProjection,
            string revision,
            Func<T, T> objectMutator = null) where T : class
        {
            string collection = _modelBuilderOptions.GetCollectionName<T>();

            await _client.CreateDocumentsAsync(
                collection,
                entities.Select(
                        e =>
                            GetJsonObjectWithInjectedKey(
                                objectMutator?.Invoke(e.Value) ?? e.Value,
                                keyProjection,
                                revision))
                    .ToList());

            await AssignTags(entities, keyProjection, revision);
        }

        private async Task CreateProfiles(IList<ExtendedProfile> profiles, string testCaseName)
        {
            await CreateEntities(
                profiles.Cast<ExtendedEntity<IFirstLevelProjectionProfile>>().ToList(),
                f => f.Id,
                testCaseName);

            string profilesCollection = _modelBuilderOptions.GetCollectionName<IFirstLevelProjectionProfile>();

            string clientSettingsCollection =
                _modelBuilderOptions.GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            string edgeCollection = _modelBuilderOptions
                .GetRelation<IFirstLevelProjectionProfile,
                    FirstLevelProjectionsClientSetting>()
                ?.EdgeCollection;

            IList<FirstLevelProjectionClientSettingsBasic> clientSettings = profiles.SelectMany(
                    p => p.ClientSettings.Select(
                        entry => new FirstLevelProjectionClientSettingsBasic
                        {
                            Key = entry.Key,
                            Value = entry.Value,
                            ProfileId = p.Value.Id
                        }))
                .ToList();

            foreach (FirstLevelProjectionClientSettingsBasic firstLevelProjectionClientSettingsBasic in clientSettings)
            {
                CreateDocumentResponse response = await _client.CreateDocumentAsync(
                    clientSettingsCollection,
                    firstLevelProjectionClientSettingsBasic);

                var edge = new Dictionary<string, object>
                {
                    {
                        AConstants.SystemPropertyFrom,
                        $"{profilesCollection}/{firstLevelProjectionClientSettingsBasic.ProfileId}"
                    },
                    { AConstants.SystemPropertyTo, response.Result.Id }
                };

                await _client.CreateDocumentAsync(edgeCollection, edge);
            }
        }

        private async Task CreateFunctions(
            IList<ExtendedEntity<FirstLevelProjectionFunction>> functions,
            string testCaseName)
        {
            string profilesCollection = _modelBuilderOptions.GetCollectionName<IFirstLevelProjectionProfile>();
            string functionsCollection = _modelBuilderOptions.GetCollectionName<FirstLevelProjectionFunction>();
            string rolesCollection = _modelBuilderOptions.GetCollectionName<FirstLevelProjectionRole>();

            string functionRoleEdge = _modelBuilderOptions
                .GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionRole>()
                .EdgeCollection;

            string functionOrgEdge =
                _modelBuilderOptions.GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionOrganization>()
                    .EdgeCollection;

            List<Dictionary<string, object>> roleRelations = functions.Select(
                    e => new Dictionary<string, object>
                    {
                        { AConstants.SystemPropertyFrom, $"{functionsCollection}/{e.Value.Id}" },
                        { AConstants.SystemPropertyTo, $"{rolesCollection}/{e.Value.Role.Id}" },
                        {
                            WellKnownAqlQueries
                                .RevisionProperty,
                            testCaseName
                        }
                    })
                .ToList();

            await _client.CreateDocumentsAsync(functionRoleEdge, roleRelations);

            List<Dictionary<string, object>> orgRelations = functions.Select(
                    e => new Dictionary<string, object>
                    {
                        { AConstants.SystemPropertyFrom, $"{functionsCollection}/{e.Value.Id}" },
                        { AConstants.SystemPropertyTo, $"{profilesCollection}/{e.Value.Organization.Id}" },
                        {
                            WellKnownAqlQueries
                                .RevisionProperty,
                            testCaseName
                        }
                    })
                .ToList();

            await _client.CreateDocumentsAsync(functionOrgEdge, orgRelations);

            await CreateEntities(
                functions,
                f => f.Id,
                testCaseName,
                f =>
                {
                    f.Role = null;
                    f.Organization = null;

                    return f;
                });
        }

        private bool TryGetJsonObjectWithInjectedKey(
            object document,
            string keyName,
            string revision,
            out JObject result)
        {
            PropertyInfo keyProp = document.GetType()
                .GetProperty(
                    keyName,
                    BindingFlags.Instance | BindingFlags.Public);

            if (keyProp == null || keyProp.PropertyType != typeof(string))
            {
                result = null;

                return false;
            }

            result = MergeDocument(
                document,
                new Dictionary<string, string>
                {
                    { AConstants.KeySystemProperty, (string)keyProp.GetValue(document) },
                    { WellKnownAqlQueries.RevisionProperty, revision }
                });

            return true;
        }

        private JObject GetJsonObjectWithInjectedKey<TDocument>(
            TDocument document,
            Func<TDocument, string> keyProjection,
            string revision)
        {
            JObject o = MergeDocument(
                document,
                new Dictionary<string, string>
                {
                    { AConstants.KeySystemProperty, keyProjection.Invoke(document) },
                    { WellKnownAqlQueries.RevisionProperty, revision }
                });

            return o;
        }

        private JObject MergeDocument(object firstDocument, object secondDocument)
        {
            var serializer = JsonSerializer.Create(
                new JsonSerializerSettings
                {
                    Converters =
                    {
                        new StringEnumConverter()
                    }
                });

            JObject first = JObject.FromObject(firstDocument, serializer);
            JObject second = JObject.FromObject(secondDocument, serializer);

            first.Merge(
                second,
                new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

            return first;
        }

        private async Task AssignTags<T>(
            IList<ExtendedEntity<T>> entities,
            Func<T, string> keyProjection,
            string testCaseName)
        {
            if (entities.All(e => !e.TagAssignments?.Any() ?? false))
            {
                return;
            }

            // create TagLink edge
            string edgeCollection = _modelBuilderOptions.GetRelation<T, FirstLevelProjectionTag>()?.EdgeCollection;
            string sourceCollection = _modelBuilderOptions.GetCollectionName<T>();
            string tagCollection = _modelBuilderOptions.GetCollectionName<FirstLevelProjectionTag>();

            List<Dictionary<string, object>> documents = entities.SelectMany(
                    e => e.TagAssignments.Select(
                        ta => new Dictionary<string, object>
                        {
                            { AConstants.SystemPropertyFrom, $"{sourceCollection}/{keyProjection.Invoke(e.Value)}" },
                            { AConstants.SystemPropertyTo, $"{tagCollection}/{ta.TagDetails.Id}" },
                            {
                                nameof(TagAssignment
                                    .IsInheritable),
                                ta.IsInheritable
                            },
                            {
                                WellKnownAqlQueries
                                    .RevisionProperty,
                                testCaseName
                            }
                        }))
                .ToList();

            await _client.CreateDocumentsAsync(edgeCollection, documents);
        }

        private async Task CreateAssignments<TTarget>(
            List<Assignment> assignments,
            ObjectType targetType,
            string testCaseName)
        {
            if (!assignments.Any())
            {
                return;
            }

            IEnumerable<Assignment> groupedAssignments = assignments
                .GroupBy(a => (a.TargetId, a.ProfileId))
                .Select(
                    grp => new Assignment
                    {
                        ProfileId = grp.Key.ProfileId,
                        TargetId = grp.Key.TargetId,
                        TargetType = targetType,
                        Conditions =
                            grp.SelectMany(ass => ass.Conditions)
                                .ToArray()
                    });

            string edgeCollection = _modelBuilderOptions.GetRelation<IFirstLevelProjectionProfile, TTarget>()
                .EdgeCollection;

            string targetCollection = _modelBuilderOptions.GetCollectionName<TTarget>();
            string profilesCollection = _modelBuilderOptions.GetCollectionName<IFirstLevelProjectionProfile>();

            List<Dictionary<string, object>> documents = groupedAssignments.Select(
                    a => new Dictionary<string, object>
                    {
                        {
                            AConstants
                                .SystemPropertyFrom,
                            $"{profilesCollection}/{a.ProfileId}"
                        },
                        { AConstants.SystemPropertyTo, $"{targetCollection}/{a.TargetId}" },
                        {
                            WellKnownAqlQueries
                                .RevisionProperty,
                            testCaseName
                        },
                        {
                            nameof(
                                Assignment.Conditions),
                            a.Conditions
                        }
                    })
                .ToList();

            await _client.CreateDocumentsAsync(edgeCollection, documents);
        }

        public Task SeedData(ObjectTestData testData)
        {
            return CreateEntities(
                testData.Data,
                testData.EntityType,
                testData.KeyPropertyName,
                testData.Name,
                testData.TestScope);
        }

        public async Task SeedData(ITestData data)
        {
            await CreateEntities(data.Tags, t => t.Id, data.Name);

            await CreateEntities(data.Roles, r => r.Id, data.Name);

            await CreateFunctions(
                data.Functions,
                data.Name);

            // create functions
            IList<ExtendedProfile> profiles = data.Profiles;
            await CreateProfiles(profiles, data.Name);

            // assignments
            List<Assignment> assignments = data.Profiles.SelectMany(p => p.Assignments).ToList();

            await CreateAssignments<FirstLevelProjectionRole>(
                assignments.Where(a => a.TargetType == ObjectType.Role).ToList(),
                ObjectType.Role,
                data.Name);

            await CreateAssignments<FirstLevelProjectionFunction>(
                assignments.Where(a => a.TargetType == ObjectType.Function).ToList(),
                ObjectType.Function,
                data.Name);

            await CreateAssignments<FirstLevelProjectionGroup>(
                assignments.Where(a => a.TargetType.IsProfileType()).ToList(),
                ObjectType.Profile,
                data.Name);

            // temporary assignments
            await CreateEntities(
                data.TemporaryAssignments,
                a => a.Id,
                data.Name);
        }
    }
}
