using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An implementation of <see cref="ICollectionDetailsProvider" /> responsible for providing the required collections
///     of the <see cref="ArangoTicketStore" />.
/// </summary>
internal class TicketStoreCollectionsProvider : ICollectionDetailsProvider
{
    /// <summary>
    ///     Prefix to be used before collection names.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="TicketStoreCollectionsProvider" />.
    /// </summary>
    /// <param name="prefix">The prefix used for the collections</param>
    public TicketStoreCollectionsProvider(string prefix)
    {
        Prefix = prefix;
    }

    /// <inheritdoc cref="ICollectionDetailsProvider.GetCollectionDetails" />
    public IEnumerable<CollectionDetails> GetCollectionDetails()
    {
        ModelBuilderOptions modelInfo = DefaultModelConstellation.CreateNewTicketStore(Prefix).ModelsInfo;

        return modelInfo.GetDocumentCollections()
            .Union(modelInfo.GetQueryDocumentCollections())
            .Select(
                col => new CollectionDetails
                {
                    CollectionName = col,
                    CollectionType = ACollectionType.Document
                })
            .Union(
                modelInfo.GetEdgeCollections()
                    .Select(
                        edge =>
                            new CollectionDetails
                            {
                                CollectionName = edge,
                                CollectionType = ACollectionType.Edge
                            }));
    }
}
