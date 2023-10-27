using System;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal interface IArangoDbEnumerable
{
    ArangoDbEnumerable GetEnumerable();
    IArangoDbQueryResult Compile<TEntity>(CollectionScope scope);
    Type GetInnerType();
}

internal interface IArangoDbEnumerable<TEntity> : IArangoDbEnumerable
{
    ArangoDbEnumerable<TEntity> GetTypedEnumerable();
    IArangoDbQueryResult Compile(CollectionScope scope);
}
