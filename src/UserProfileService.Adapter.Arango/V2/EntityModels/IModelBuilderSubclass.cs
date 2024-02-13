using System.Collections.Generic;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a model builder subclass.
/// </summary>
public interface IModelBuilderSubclass
{
    /// <summary>
    ///     Gets the list of child model builder subclasses.
    /// </summary>
    List<IModelBuilderSubclass> Children { get; }

    /// <summary>
    ///     Gets the model builder options.
    /// </summary>
    ModelBuilderOptions Options { get; }

    /// <summary>
    ///     Gets the parent model builder subclass.
    /// </summary>
    IModelBuilderSubclass Parent { get; }

    /// <summary>
    ///     Builds the model using the specified collection prefixes.
    /// </summary>
    /// <param name="collectionPrefix">The collection prefix for building.</param>
    /// <param name="queryCollectionPrefix">The collection prefix for queries.</param>
    void Build(string collectionPrefix, string queryCollectionPrefix);
}
