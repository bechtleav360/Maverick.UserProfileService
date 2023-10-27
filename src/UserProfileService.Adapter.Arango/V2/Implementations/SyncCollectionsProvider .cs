using System.Collections.Generic;
using System.Linq;
using Maverick.Client.ArangoDb.Public;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class SyncCollectionsProvider : ICollectionDetailsProvider
{
    private readonly string _prefix;
   


    /// <summary>
    ///     Initializes a new <see cref="UserProfileStoreCollectionsProvider" /> with two different prefixes for query and
    ///     command collections.
    /// </summary>
    /// <param name="prefix">The prefix for command collections.</param>
   
    public SyncCollectionsProvider(string prefix)
    {
        _prefix = prefix;
    }

    private static IEnumerable<CollectionDetails> GetDocumentCollections(ModelBuilderOptions modelInfo)
    {
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

    /// <inheritdoc />
    public IEnumerable<CollectionDetails> GetCollectionDetails()
    {
        ModelBuilderOptions syncModelInfo = DefaultModelConstellation.CreateNewSyncStore(_prefix).ModelsInfo;

        ModelBuilderOptions syncScheduleModelInfo =
            DefaultModelConstellation.CreateSyncScheduleStore(_prefix).ModelsInfo;

        ModelBuilderOptions syncEntityModelInfo =
            DefaultModelConstellation.CreateSyncEntityStore(_prefix).ModelsInfo;

        IEnumerable<CollectionDetails> syncCollectionDetails = GetDocumentCollections(syncModelInfo);
        IEnumerable<CollectionDetails> scheduleCollectionDetails = GetDocumentCollections(syncScheduleModelInfo);
        IEnumerable<CollectionDetails> entityCollectionDetails = GetDocumentCollections(syncEntityModelInfo);

        return syncCollectionDetails
            .Concat(scheduleCollectionDetails)
            .Concat(entityCollectionDetails);
    }
}
