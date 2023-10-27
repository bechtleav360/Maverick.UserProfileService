namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class ArangoDbSingleEnumerable<TEntity> : ArangoDbEnumerable<TEntity>, IArangoDbSingleEnumerable<TEntity>
{
    public ArangoDbSingleEnumerable(
        string newId,
        IArangoDbEnumerable current,
        object settings) : base(newId, current, settings)
    {
    }

    public ArangoDbSingleEnumerable(
        IArangoDbEnumerable current,
        object settings) : base(current, settings)
    {
    }

    public ArangoDbSingleEnumerable(
        string newId,
        IArangoDbEnumerable current) : base(newId, current)
    {
    }

    public ArangoDbSingleEnumerable(IArangoDbEnumerable current) : base(current)
    {
    }

    public ArangoDbSingleEnumerable(ModelBuilderOptions modelSettings) : base(modelSettings)
    {
    }
}
