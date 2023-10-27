using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An implementation of <see cref="ICollectionDetailsProvider" /> responsible for providing the required collections
///     of the <see cref="ArangoEventCollectorStore" />.
/// </summary>
internal class EventCollectorCollectionsProvider : ICollectionDetailsProvider
{
    /// <summary>
    ///     Prefix to be used before collection names.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="EventCollectorCollectionsProvider" />.
    /// </summary>
    /// <param name="prefix">The prefix used for the collections</param>
    public EventCollectorCollectionsProvider(string prefix)
    {
        Prefix = prefix;
    }

    /// <inheritdoc cref="ICollectionDetailsProvider.GetCollectionDetails" />
    public IEnumerable<CollectionDetails> GetCollectionDetails()
    {
        ModelBuilderOptions modelInfo = DefaultModelConstellation.CreateNewEventCollectorStore(Prefix).ModelsInfo;

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
