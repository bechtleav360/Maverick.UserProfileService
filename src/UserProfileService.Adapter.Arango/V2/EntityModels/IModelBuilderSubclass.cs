using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal interface IModelBuilderSubclass
{
    List<IModelBuilderSubclass> Children { get; }
    ModelBuilderOptions Options { get; }
    IModelBuilderSubclass Parent { get; }

    void Build(string collectionPrefix, string queryCollectionPrefix);
}
