using System;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IModelBuilder
{
    IModelBuilderEntityOptions<TEntity> Entity<TEntity>();

    string GetCollectionName<TEntity>();

    string GetCollectionName(Type type);

    ModelBuilderOptions BuildOptions(string collectionPrefix, string queryCollectionPrefix = null);
}
