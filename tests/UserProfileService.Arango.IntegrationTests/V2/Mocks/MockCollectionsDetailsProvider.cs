using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Arango.IntegrationTests.V2.Mocks
{
    internal class MockCollectionsDetailsProvider : ICollectionDetailsProvider
    {
        private readonly string _collectionName;
        private readonly ACollectionType _collectionsType;
        public string Prefix { get; }

        public MockCollectionsDetailsProvider(string collectionName, ACollectionType collectionsType, string prefix)
        {
            _collectionName = collectionName;
            _collectionsType = collectionsType;
            Prefix = prefix;
        }

        public IEnumerable<CollectionDetails> GetCollectionDetails()
        {
            return new List<CollectionDetails>
            {
                new CollectionDetails
                {
                    CollectionName = $"{Prefix}{_collectionName}",
                    CollectionType = _collectionsType
                }
            };
        }
    }
}
