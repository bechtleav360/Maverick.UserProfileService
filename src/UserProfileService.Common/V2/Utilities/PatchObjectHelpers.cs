using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Abstraction;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Contains methods to change the state of objects or entities (like properties).
/// </summary>
public static class PatchObjectHelpers
{
    private static void Apply(
        object toBeModified,
        PropertyInfo propertyInfo,
        object newValue)
    {
        try
        {
            if (newValue == null
                && propertyInfo.PropertyType.IsValueType
                && !IsNullableValueType(propertyInfo.PropertyType, out _))
            {
                throw new NotValidException(
                    $"Could not set property '{propertyInfo.Name}' to 'null', because target property type is not nullable. (type of property: {propertyInfo.PropertyType.FullName})");
            }

            if (newValue == null)
            {
                // value can be null to "delete" the old value
                propertyInfo.SetValue(toBeModified, null, null);

                return;
            }
        }
        catch (Exception exception)
        {
            throw new NotValidException(
                $"Could not set new value of property '{propertyInfo.Name}', because an error occurred during setting the new value. (new value: null; type of property: {propertyInfo.PropertyType.FullName})",
                exception);
        }

        Type valueType = newValue.GetType();

        try
        {
            if (IsNullableValueType(
                    propertyInfo.PropertyType,
                    out Type innerType)
                && innerType == valueType)
            {
                propertyInfo.SetValue(toBeModified, newValue, null);

                return;
            }

            // value of dictionary can be set directly, because types of existing and new values are compatible
            if (propertyInfo.PropertyType == valueType
                || valueType.IsAssignableFrom(propertyInfo.PropertyType))
            {
                propertyInfo.SetValue(toBeModified, newValue, null);

                return;
            }

            if (valueType.IsAssignableFromGeneric(propertyInfo.PropertyType))
            {
                propertyInfo.SetValue(toBeModified, newValue, null);

                return;
            }

            if (newValue is JContainer jsonValue)
            {
                propertyInfo.SetValue(toBeModified, jsonValue.ToObject(propertyInfo.PropertyType));

                return;
            }

            propertyInfo.SetValue(
                toBeModified,
                Conversion(newValue, propertyInfo.PropertyType),
                null);
        }
        catch (InvalidCastException invalidCastException)
        {
            throw new NotValidException(
                $"Could not set new value of property '{propertyInfo.Name}', because the value type cannot be cast/converted to property type. (type of new value: {valueType.FullName}; type of property: {propertyInfo.PropertyType.FullName})",
                invalidCastException);
        }
        catch (NotValidException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new NotValidException(
                $"Could not set new value of property '{propertyInfo.Name}', because the types are not compatible and could not be cast. (type of new value: {valueType.FullName}; type of property: {propertyInfo.PropertyType.FullName})",
                exception);
        }
    }

    private static bool IsNullableValueType(
        Type type,
        out Type wrappedValueType)
    {
        if (!type.IsGenericType
            || type.GenericTypeArguments.Length != 1
            || !type.GenericTypeArguments.First().IsValueType)
        {
            wrappedValueType = null;

            return false;
        }

        Type nullableType = typeof(Nullable<>).MakeGenericType(type.GenericTypeArguments.First());

        wrappedValueType = nullableType.GenericTypeArguments.First();

        return nullableType.IsAssignableFrom(type);
    }

    private static object Conversion(object toBeConverted, Type propertyType)
    {
        if ((toBeConverted.GetType().IsClass && propertyType.IsValueType)
            || (propertyType.IsClass && toBeConverted.GetType().IsValueType))
        {
            throw new NotValidException(
                $"Cannot set property '{propertyType.Name}' to value with type {toBeConverted.GetType().FullName}, because they are not compatible (reference type <-> value type).");
        }

        return Convert.ChangeType(toBeConverted, propertyType);
    }

    /// <summary>
    ///     Modifies a provided object with a set of changed properties.
    /// </summary>
    /// <param name="toBeModified">The object to be modified.</param>
    /// <param name="changedProperties">
    ///     A set of key-value-pairs that represents the requested changes of
    ///     <paramref name="toBeModified" />.
    /// </param>
    /// <param name="upsEvent">The related event</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="changedProperties" /> is null<br /> -or- <br />
    ///     <paramref name="toBeModified" /> is null
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="changedProperties" /> does not contain any key-value-pair<br /> -or- <br />
    ///     <paramref name="changedProperties" /> does not contain any key-value-pair that can be matched to the properties of
    ///     <paramref name="toBeModified" />
    /// </exception>
    /// <exception cref="NotValidException">
    ///     <paramref name="toBeModified" /> could not be modified, because of an error during setting at least one property
    ///     value. This can be happen, because of an invalid cast (i.e. trying to set base type to a property of its derived
    ///     type).
    /// </exception>
    public static void ApplyPropertyChanges(
        object toBeModified,
        IReadOnlyDictionary<string, object> changedProperties,
        IUserProfileServiceEvent upsEvent)
    {
        if (toBeModified == null)
        {
            throw new ArgumentNullException(nameof(toBeModified));
        }

        if (changedProperties == null)
        {
            throw new ArgumentNullException(nameof(changedProperties));
        }

        if (changedProperties.Count == 0)
        {
            throw new ArgumentException("changedProperties cannot be empty.", nameof(changedProperties));
        }

        PropertyInfo[] propertiesOfObjects =
            toBeModified.GetType()
                .GetProperties(
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.SetProperty)
                .Where(
                    p => !string.IsNullOrWhiteSpace(p.Name) // to be absolutely on the safe side
                        && changedProperties.ContainsKey(p.Name))
                .ToArray();

        if (propertiesOfObjects.Length == 0)
        {
            throw new ArgumentException(
                "No properties found that can be set and suit to changed properties dictionary.");
        }

        foreach (PropertyInfo propertyInfo in propertiesOfObjects)
        {
            Apply(
                toBeModified,
                propertyInfo,
                changedProperties[propertyInfo.Name]);
        }

        PropertyInfo propertyUpdatedAt =
            propertiesOfObjects.SingleOrDefault(p => p.Name == nameof(IProfile.UpdatedAt));

        if (propertyUpdatedAt != null)
        {
            if (upsEvent == null)
            {
                throw new ArgumentNullException(
                    nameof(upsEvent),
                    $"The event must not be null if the object to be overwritten has a property of name {nameof(IProfile.UpdatedAt)}.");
            }

            Apply(toBeModified, propertyUpdatedAt, upsEvent.MetaData.Timestamp);
        }
    }
}
