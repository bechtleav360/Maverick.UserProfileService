using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ModelBuilderRelationBuilder<TFrom, TTo> : IRelationBuilder<TFrom, TTo>
{
    public string CollectionName { get; set; } = $"{typeof(TFrom).Name}{typeof(TTo).Name}";
    public IList<string> FromExtraProperties { get; set; } = new List<string>();
    public IList<string> ToExtraProperties { get; set; } = new List<string>();

    public IRelationBuilder WithCollectionName(string collectionName)
    {
        CollectionName = collectionName;

        return this;
    }

    public IRelationBuilder WithFromProperty(string propertyName)
    {
        FromExtraProperties.Add(propertyName);

        return this;
    }

    public IRelationBuilder WithToProperty(string propertyName)
    {
        ToExtraProperties.Add(propertyName);

        return this;
    }

    public ModelBuilderOptionsTypeRelation Build(IModelBuilderSubclass parent)
    {
        return ModelBuilderOptionsTypeRelation.Create<TFrom, TTo>(
            CollectionName,
            parent,
            FromExtraProperties,
            ToExtraProperties);
    }

    public IRelationBuilder<TFrom, TTo> WithFromProperty<TProp>(Expression<Func<TFrom, TProp>> propertySelector)
    {
        if (propertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(propertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException("Cannot extract member name from expression.", nameof(propertySelector));
        }

        WithFromProperty(memberExpression.Member.Name);

        return this;
    }

    public IRelationBuilder<TFrom, TTo> WithToProperty<TProp>(Expression<Func<TTo, TProp>> propertySelector)
    {
        if (propertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(propertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException("Cannot extract member name from expression.", nameof(propertySelector));
        }

        WithToProperty(memberExpression.Member.Name);

        return this;
    }
}
