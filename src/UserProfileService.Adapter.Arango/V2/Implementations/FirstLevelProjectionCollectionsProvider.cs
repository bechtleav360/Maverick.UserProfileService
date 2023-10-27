using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class FirstLevelProjectionCollectionsProvider : ICollectionDetailsProvider
{
    private readonly string _prefix;

    /// <summary>
    ///     Initializes a new <see cref="FirstLevelProjectionCollectionsProvider" />.
    /// </summary>
    /// <param name="prefix">The prefix for command collections.</param>
    public FirstLevelProjectionCollectionsProvider(string prefix)
    {
        _prefix = prefix;
    }

    /// <inheritdoc />
    public IEnumerable<CollectionDetails> GetCollectionDetails()
    {
        ModelBuilderOptions modelInfo = DefaultModelConstellation.CreateNewFirstLevelProjection(_prefix).ModelsInfo;

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
