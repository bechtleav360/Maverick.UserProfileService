using Maverick.Client.ArangoDb.Public.Extensions;
using Newtonsoft.Json;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    internal static class ClientFactoryExtensions
    {
        internal static SingletonArangoDbClientFactory AddClient(
            this SingletonArangoDbClientFactory factory,
            string name,
            params JsonConverter[] converters)
        {
            factory.RegisterClient(
                name,
                "endpoints=http://localhost:8529;UserName=test;Password=test;database=ups",
                settings: new JsonSerializerSettings
                {
                    Converters = converters
                });

            return factory;
        }
    }
}
