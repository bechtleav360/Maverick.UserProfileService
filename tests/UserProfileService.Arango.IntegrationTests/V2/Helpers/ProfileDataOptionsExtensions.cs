using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    internal static class ProfileDataOptionsExtensions
    {
        internal static async Task ProjectOnArangoDbReadPartAsync(
            this ProfileDataOptions dataOptions,
            IArangoDbClient client,
            ModelBuilderOptions options,
            JsonSerializer jsonSerializer)
        {
            // ensure collections
            IEnumerable<string> queryCollections = new[]
                {
                    options.GetQueryCollectionName(typeof(UserEntityModel)),
                    options.GetQueryCollectionName(typeof(GroupEntityModel)),
                    options.GetQueryCollectionName(typeof(RoleObjectEntityModel)),
                    options.GetQueryCollectionName(typeof(FunctionObjectEntityModel))
                }
                .Distinct();

            foreach (string name in queryCollections)
            {
                CreateCollectionResponse response = await client.CreateCollectionAsync(name);

                // if collection is already there, ignore the error
                if (response.Error && response.Code != HttpStatusCode.Conflict)
                {
                    throw new Exception("Error occurred during test seeding data.", response.Exception);
                }
            }

            foreach (IProfileEntityModel profile in dataOptions.Profiles)
            {
                await client.CreateDocumentAsync(
                    options.GetQueryCollectionName(profile.GetType()),
                    profile.GetJsonObjectWithInjectedKey(
                        jsonSerializer,
                        d => d.Id));
            }

            foreach (IAssignmentObjectEntity functionOrRole in dataOptions.FunctionsAndRoles)
            {
                await client.CreateDocumentAsync(
                    options.GetQueryCollectionName(functionOrRole.GetType()),
                    functionOrRole.GetJsonObjectWithInjectedKey(
                        jsonSerializer,
                        d => d.Id));
            }
        }

        internal static Task ProjectOnArangoDbWritePartAsync(
            this ProfileDataOptions dataOptions,
            IArangoDbClient client,
            ModelBuilderOptions options,
            JsonSerializer jsonSerializer)
        {
            return Task.CompletedTask;
        }
    }
}
