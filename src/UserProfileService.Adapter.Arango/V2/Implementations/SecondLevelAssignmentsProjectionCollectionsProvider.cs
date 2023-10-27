using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class SecondLevelAssignmentsProjectionCollectionsProvider : ICollectionDetailsProvider
{
    private readonly string _prefix;
    private readonly string _queryPrefix;

    /// <summary>
    ///     Initializes a new <see cref="SecondLevelProjectionCollectionsProvider" />.
    /// </summary>
    /// <param name="prefix">The prefix for command collections.</param>
    public SecondLevelAssignmentsProjectionCollectionsProvider(string prefix)
    {
        _prefix = prefix;
    }

    /// <summary>
    ///     Initializes a new <see cref="SecondLevelProjectionCollectionsProvider" />.
    /// </summary>
    /// <param name="prefix">The prefix for command collections.</param>
    /// <param name="queryPrefix">The query prefix to use.</param>
    public SecondLevelAssignmentsProjectionCollectionsProvider(string prefix, string queryPrefix)
    {
        _prefix = prefix;
        _queryPrefix = queryPrefix;
    }

    /// <inheritdoc />
    public IEnumerable<CollectionDetails> GetCollectionDetails()
    {
        ModelBuilderOptions modelInfo =
            DefaultModelConstellation.NewAssignmentsProjectionRepository(_prefix, _queryPrefix).ModelsInfo;

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
