namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IModelBuilderEntityQueryOptions
{
    IModelBuilderEntityQueryOptions Collection<TAliasEntity>(string collectionName);
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo>(string collectionName);

    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo, TAliasEntityThree>(
        string collectionName);

    void QueryCollection(string collectionName);
}
