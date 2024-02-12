using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Arango.UnitTests.V2.Mocks
{
    internal class MockCollectionsDetailsProvider : ICollectionDetailsProvider
    {
        public string Prefix { get; }

        public MockCollectionsDetailsProvider(string prefix)
        {
            Prefix = prefix;
        }

        public IEnumerable<CollectionDetails> GetCollectionDetails()
        {
            return new List<CollectionDetails>
            {
                new CollectionDetails
                {
                    CollectionName =
                        "Test",
                    CollectionType = ACollectionType.Document
                }
            };
        }
    }
}
