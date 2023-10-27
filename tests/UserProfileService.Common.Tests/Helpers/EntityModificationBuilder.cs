using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UserProfileService.Common.Tests.Helpers
{
    public class EntityModificationBuilder
    {
        public static EntityModificationBuilder<TEntity> Create<TEntity>(
            TEntity entity,
            Func<TEntity, TEntity> cloneFunction)
        {
            return new EntityModificationBuilder<TEntity>(entity, cloneFunction.Invoke(entity));
        }

        public static EntityModificationBuilder<TEntity> Create<TEntity>(
            TEntity entity)
        {
            return new EntityModificationBuilder<TEntity>(entity, default);
        }
    }

    public class EntityModificationBuilder<TEntity>
    {
        private readonly Dictionary<string, object> _changeSet;
        public IReadOnlyDictionary<string, object> ChangeSet => _changeSet;
        public TEntity ModifiedEntity { get; }
        public TEntity OriginalEntity { get; }

        public EntityModificationBuilder(
            TEntity entity,
            TEntity clone)
        {
            _changeSet = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            OriginalEntity = entity;
            ModifiedEntity = clone;
        }

        private static PropertyInfo GetPropertyInfo(Expression<Func<TEntity, object>> propertySelector)
        {
            MemberExpression me;

            if (propertySelector.Body is UnaryExpression unary)
            {
                me = unary.Operand as MemberExpression;
            }
            else
            {
                me = propertySelector.Body as MemberExpression;
            }

            return me?.Member is PropertyInfo pi
                ? pi
                : throw new ArgumentException("propertySelector wrong!");
        }

        private void LogChange(
            string propertyName,
            object newValue)
        {
            if (_changeSet.ContainsKey(propertyName))
            {
                _changeSet[propertyName] = newValue;

                return;
            }

            _changeSet.Add(propertyName, newValue);
        }

        private static bool TryGetPropertyInfo(string propertyName, out PropertyInfo propertyInfo)
        {
            List<PropertyInfo> found = typeof(TEntity)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!found.Any())
            {
                propertyInfo = default;

                return false;
            }

            propertyInfo = found.First();

            return true;
        }

        public EntityModificationBuilder<TEntity> AddChange(
            Expression<Func<TEntity, object>> propertySelector,
            object newValue)
        {
            if (ModifiedEntity == null)
            {
                throw new NotSupportedException("ModifiedEntity is null. Therefore this method is not supported.");
            }

            PropertyInfo propertyInfo = GetPropertyInfo(propertySelector);
            propertyInfo.SetValue(ModifiedEntity, newValue);
            LogChange(propertyInfo.Name, newValue);

            return this;
        }

        public EntityModificationBuilder<TEntity> AddInvalidChange(
            Expression<Func<TEntity, object>> propertySelector,
            object newValue)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertySelector);
            LogChange(propertyInfo.Name, newValue);

            return this;
        }

        public EntityModificationBuilder<TEntity> AddChange(
            Expression<Func<TEntity, object>> propertySelector,
            Func<TEntity, object> newValueSelector)
        {
            if (ModifiedEntity == null)
            {
                throw new NotSupportedException("ModifiedEntity is null. Therefore this method is not supported.");
            }

            PropertyInfo propertyInfo = GetPropertyInfo(propertySelector);
            object newValue = newValueSelector.Invoke(OriginalEntity);
            propertyInfo.SetValue(ModifiedEntity, newValue);
            LogChange(propertyInfo.Name, newValue);

            return this;
        }

        public EntityModificationBuilder<TEntity> AddChange(
            string propertyName,
            object newValue)
        {
            if (ModifiedEntity == null)
            {
                throw new NotSupportedException("ModifiedEntity is null. Therefore this method is not supported.");
            }

            if (TryGetPropertyInfo(propertyName, out PropertyInfo propertyInfo))
            {
                propertyInfo.SetValue(ModifiedEntity, newValue);
            }

            LogChange(propertyName, newValue);

            return this;
        }
    }
}
