namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal interface IArangoDbSingleEnumerable<TEntity>
{
    ArangoDbEnumerable<TEntity> GetTypedEnumerable();
}
