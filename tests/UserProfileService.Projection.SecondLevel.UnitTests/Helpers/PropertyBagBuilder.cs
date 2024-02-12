using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

public class PropertyBagBuilder<TEntity> where TEntity : class
{
    private readonly IDictionary<string, object> _properties =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public PropertyBagBuilder<TEntity> AddChange<TValue>(
        Expression<Func<TEntity, TValue>> propertySelector,
        TValue newValue)
    {
        string propertyName = ((MemberExpression)propertySelector.Body).Member.Name;

        return AddChange(propertyName, newValue);
    }

    public PropertyBagBuilder<TEntity> AddChange(
        string propertyName,
        object newValue)
    {
        if (_properties.ContainsKey(propertyName))
        {
            return this;
        }

        _properties.Add(propertyName, newValue);

        return this;
    }

    public IDictionary<string, object> Build()
    {
        return _properties;
    }
}