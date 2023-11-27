using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IRelationBuilder
{
    IRelationBuilder WithCollectionName(string collectionName);
    IRelationBuilder WithToProperty(string propertyName);
    ModelBuilderOptionsTypeRelation Build(IModelBuilderSubclass parent);
}

public interface IRelationBuilder<TFrom, TTo> : IRelationBuilder
{
    IRelationBuilder<TFrom, TTo> WithFromProperty<TProp>(Expression<Func<TFrom, TProp>> propertySelector);
    IRelationBuilder<TFrom, TTo> WithToProperty<TProp>(Expression<Func<TTo, TProp>> propertySelector);
}
