using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a builder for defining relations between entity types.
/// </summary>
public interface IRelationBuilder
{
    /// <summary>
    ///     Specifies the collection name for the relation.
    /// </summary>
    /// <param name="collectionName">The name of the edge collection.</param>
    /// <returns>The relation builder for further configuration.</returns>
    IRelationBuilder WithCollectionName(string collectionName);

    /// <summary>
    ///     Specifies the property on the "to" side of the relation.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The relation builder for further configuration.</returns>
    IRelationBuilder WithToProperty(string propertyName);

    /// <summary>
    ///     Builds the type relation configuration.
    /// </summary>
    /// <param name="parent">The parent model builder subclass.</param>
    /// <returns>The configured type relation configuration.</returns>
    ModelBuilderOptionsTypeRelation Build(IModelBuilderSubclass parent);
}

/// <summary>
///     Represents a builder for defining relations between types with specific "from" and "to" entity types.
/// </summary>
/// <typeparam name="TFrom">The "from" type.</typeparam>
/// <typeparam name="TTo">The "to" type.</typeparam>
public interface IRelationBuilder<TFrom, TTo> : IRelationBuilder
{
    /// <summary>
    ///     Specifies the property on the "from" side of the relation.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="propertySelector">An expression that selects the property.</param>
    /// <returns>The relation builder for further configuration.</returns>
    IRelationBuilder<TFrom, TTo> WithFromProperty<TProp>(Expression<Func<TFrom, TProp>> propertySelector);

    /// <summary>
    ///     Specifies the property on the "to" side of the relation.
    /// </summary>
    /// <typeparam name="TProp">The type of the property.</typeparam>
    /// <param name="propertySelector">An expression that selects the property.</param>
    /// <returns>The relation builder for further configuration.</returns>
    IRelationBuilder<TFrom, TTo> WithToProperty<TProp>(Expression<Func<TTo, TProp>> propertySelector);
}
