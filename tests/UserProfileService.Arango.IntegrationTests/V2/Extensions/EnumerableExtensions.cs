using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Utilities;

namespace UserProfileService.Arango.IntegrationTests.V2.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<TElem> ConcatSeveralSequences<TElem>(
            this IEnumerable<TElem> initSequence,
            params IEnumerable<TElem>[] others)
        {
            IEnumerable<TElem> enumerable = initSequence;

            enumerable = others.Aggregate(
                enumerable,
                (
                    current,
                    objects) => current.Aggregate(
                    objects,
                    (
                        seed,
                        elem) => seed.Append(elem)));

            return enumerable;
        }

        internal static IEnumerable<TElem> OrderByIgnoreCase<TElem>(
            this IEnumerable<TElem> sequence,
            Func<TElem, string> keySelector,
            SortOrder sortingOrder)
        {
            return OrderBy(sequence, keySelector, sortingOrder, StringComparer.OrdinalIgnoreCase);
        }

        internal static IEnumerable<TElem> OrderBy<TElem, TKey>(
            this IEnumerable<TElem> sequence,
            Func<TElem, TKey> keySelector,
            SortOrder sortingOrder,
            IComparer<TKey> comparer = null)
        {
            return keySelector != null
                ? sortingOrder == SortOrder.Asc
                    ? sequence.OrderBy(keySelector, comparer)
                    : sequence.OrderByDescending(keySelector, comparer)
                : sequence;
        }

        internal static IEnumerable<TObj> SortBy<TObj>(
            this IEnumerable<TObj> sequence,
            string propertyName,
            SortOrder order)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return sequence;
            }

            PropertyInfo property = typeof(TObj)
                .GetPublicProperties()
                .FirstOrDefault(p => p.Name == propertyName);

            if (property == null)
            {
                throw new ArgumentException(
                    $"Wrong property name. Could not find {propertyName} in type {typeof(TObj).Name}");
            }

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TObj), "obj");

            Delegate innerFunc = Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(typeof(TObj), property.PropertyType),
                    Expression.Property(parameterExpression, property),
                    parameterExpression)
                .Compile();

            string expectedMethodName = order == SortOrder.Asc
                ? nameof(Enumerable.OrderBy)
                : nameof(Enumerable.OrderByDescending);

            MethodInfo orderMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(
                    m =>
                        m.Name == expectedMethodName
                        && m.GetParameters().Length == 2)
                ?
                .MakeGenericMethod(typeof(TObj), property.PropertyType);

            var ordered = orderMethod?.Invoke(null, new object[] { sequence, innerFunc })
                as IEnumerable<TObj>;

            return ordered ?? sequence;
        }

        private static IEnumerable<TElem> ApplySearchFilter<TElem>(
            this IEnumerable<TElem> elements,
            string filterString,
            Func<TElem, string, bool> searchComparer = null)
        {
            if (string.IsNullOrWhiteSpace(filterString) || searchComparer == null || elements == null)
            {
                return elements;
            }

            return elements.Where(e => searchComparer.Invoke(e, filterString));
        }

        private static int CountAfterApplyingSearchFilter<TElem>(
            this IEnumerable<TElem> elements,
            string filterString,
            Func<TElem, string, bool> searchComparer = null)
        {
            if (string.IsNullOrWhiteSpace(filterString) || searchComparer == null || elements == null)
            {
                return elements?.Count() ?? 0;
            }

            return elements.Count(e => searchComparer.Invoke(e, filterString));
        }

        private static IEnumerable<TElem> ApplyPagination<TElem>(
            this IEnumerable<TElem> elements,
            IQueryObject options)
        {
            return options == null
                ? elements
                : elements.Skip(options.Offset >= 0 ? options.Offset : 0)
                    .Take(options.Limit > 0 ? options.Limit : 100);
        }

        private static IEnumerable<TElem> ApplySorting<TElem>(
            this IEnumerable<TElem> elements,
            IQueryObject options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.OrderedBy)
                || GetPublicProperties(typeof(TElem))
                    .All(p => !p.Name.Equals(options.OrderedBy, StringComparison.OrdinalIgnoreCase)))
            {
                return elements;
            }

            return elements.SortBy(options.OrderedBy, options.SortOrder);
        }

        private static IEnumerable<TElem> WhereFilterDefinition<TElem>(
            this IEnumerable<TElem> elements,
            Definitions definition)
        {
            if (definition?.Values == null)
            {
                return elements;
            }

            PropertyInfo propertyInfo =
                typeof(TElem).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(p => p.Name.Equals(definition.FieldName));

            if (propertyInfo == null)
            {
                //throw new Exception("TEST exception: Unknown property name!");
                return elements;
            }

            List<Expression<Func<TElem, bool>>> expressions = definition.Values
                .Where(val => !string.IsNullOrWhiteSpace(val))
                .Select(val => GetExpression<TElem>(propertyInfo, val, definition.Operator))
                .ToList();

            if (expressions.Count == 0)
            {
                return elements;
            }

            List<TElem> temp = null;

            expressions.ForEach(
                e =>
                {
                    if (temp == null)
                    {
                        temp = elements.Where(e.Compile()).ToList();

                        return;
                    }

                    IEnumerable<TElem> newSubSet =
                        definition.BinaryOperator == BinaryOperator.And
                            ? elements.Where(e.Compile()).Intersect(temp)
                            : elements.Where(e.Compile()).Union(temp).Distinct();

                    temp = newSubSet.ToList();
                });

            return temp;
        }

        private static Expression<Func<TElem, bool>> GetExpression<TElem>(
            this PropertyInfo propertyInfo,
            object value,
            FilterOperator filterOperator)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TElem), "property");

            Expression binaryExpression = filterOperator switch
            {
                FilterOperator.Equals => Expression.Equal(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                FilterOperator.NotEquals => Expression.NotEqual(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                FilterOperator.LowerThan => Expression.LessThan(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                FilterOperator.GreaterThan => Expression.GreaterThan(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                FilterOperator.LowerThanEquals => Expression.LessThanOrEqual(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                FilterOperator.GreaterThanEquals => Expression.GreaterThanOrEqual(
                    Expression.Property(parameter, propertyInfo),
                    Expression.Constant(value)),
                _ => throw new ArgumentOutOfRangeException(nameof(filterOperator), filterOperator, null)
            };

            return Expression.Lambda<Func<TElem, bool>>(binaryExpression, parameter);
        }

        public static IPaginatedList<TElem> UsingQueryOptions<TElem>(
            this IEnumerable<TElem> elements,
            QueryObjectBase options)
        {
            if (options == null)
            {
                return elements == null ? new PaginatedList<TElem>() : new PaginatedList<TElem>(elements);
            }

            if (elements == null)
            {
                return new PaginatedList<TElem>();
            }

            List<TElem> validElements = elements.ToList();

            return new PaginatedList<TElem>(
                validElements
                    .ApplySorting(options)
                    .ApplyPagination(options),
                validElements.Count);
        }

        public static IPaginatedList<TElem> UsingQueryOptions<TElem>(
            this IEnumerable<TElem> elements,
            QueryObject options,
            Func<TElem, string, bool> searchComparer = null)
        {
            if (options == null)
            {
                return elements == null ? new PaginatedList<TElem>() : new PaginatedList<TElem>(elements);
            }

            if (elements == null)
            {
                return new PaginatedList<TElem>();
            }

            List<TElem> validElements = elements.ToList();

            if (options.Filter?.Definition == null)
            {
                return new PaginatedList<TElem>(
                    validElements
                        .ApplySearchFilter(options.Search, searchComparer)
                        .ApplySorting(options)
                        .ApplyPagination(options),
                    validElements.CountAfterApplyingSearchFilter(options.Search, searchComparer));
            }

            List<TElem> temp = null;

            foreach (Definitions definition in options.Filter.Definition
                         .Where(def => def != null))
            {
                if (temp == null)
                {
                    temp = validElements.WhereFilterDefinition(definition).ToList();

                    continue;
                }

                IEnumerable<TElem> newSubSet = options.Filter.CombinedBy == BinaryOperator.And
                    ? validElements.WhereFilterDefinition(definition).Intersect(temp)
                    : validElements.WhereFilterDefinition(definition).Union(temp).Distinct();

                temp = newSubSet.ToList();
            }

            return temp == null
                ? new PaginatedList<TElem>()
                : new PaginatedList<TElem>(
                    temp
                        .ApplySearchFilter(options.Search, searchComparer)
                        .ApplySorting(options)
                        .ApplyPagination(options),
                    temp.CountAfterApplyingSearchFilter(options.Search, searchComparer));
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
            if (!type.IsInterface)
            {
                return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            }

            return new[] { type }
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public));
        }
    }
}
