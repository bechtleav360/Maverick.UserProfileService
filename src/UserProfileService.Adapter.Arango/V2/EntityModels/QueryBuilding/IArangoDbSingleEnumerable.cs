namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public interface IArangoDbSingleEnumerable<TEntity>
{
    ArangoDbEnumerable<TEntity> GetTypedEnumerable();
}
