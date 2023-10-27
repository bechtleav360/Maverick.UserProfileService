using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="Type" />s.
/// </summary>
public static class TypeExtension
{
    private static bool IsDerivedArrayAndCompatibleToBase(
        Type derivedType,
        Type baseType)
    {
        if (derivedType == null
            || baseType == null
            || !derivedType.IsArray
            || !baseType.IsGenericType)
        {
            return false;
        }

        Type[] genericArgumentsOfBase = baseType.GetGenericArguments();

        if (genericArgumentsOfBase.Length != 1
            || genericArgumentsOfBase.First() != derivedType.GetElementType())
        {
            return false;
        }

        return baseType.IsAssignableFrom(derivedType);
    }

    private static bool AreBothSameGenericList(
        Type derivedType,
        Type[] genericArgumentsOfDerived,
        Type baseType,
        Type[] genericArgumentsOfBase)
    {
        if (genericArgumentsOfDerived.Length != 1
            || genericArgumentsOfBase.Length != 1)
        {
            return false;
        }

        if (genericArgumentsOfDerived.First() != genericArgumentsOfBase.First())
        {
            return false;
        }

        Type derivedListType = typeof(IList<>).MakeGenericType(genericArgumentsOfDerived);

        return
            baseType.IsAssignableFrom(derivedType)
            || baseType.IsAssignableFrom(derivedListType);
    }

    private static bool AreBothSameGenericDictionary(
        Type derivedType,
        Type[] genericArgumentsOfDerived,
        Type baseType,
        Type[] genericArgumentsOfBase)
    {
        if (genericArgumentsOfDerived.Length != 2
            || genericArgumentsOfBase.Length != 2)
        {
            return false;
        }

        if (!genericArgumentsOfDerived.SequenceEqual(genericArgumentsOfBase))
        {
            return false;
        }

        Type derivedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArgumentsOfDerived);

        return
            derivedType.IsAssignableFrom(baseType)
            || derivedDictionaryType.IsAssignableFrom(baseType);
    }

    /// <summary>
    ///     Check if the type implements the given interface.
    /// </summary>
    /// <param name="type">Type to be checked.</param>
    /// <param name="interfaceType">Interface to be checked if it is implemented.</param>
    /// <returns>True if type implements generic interface, otherwise false.</returns>
    public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
    {
        return type.GetGenericInterface(interfaceType) != null;
    }

    /// <summary>
    ///     Check if the type implements the given interface with the given generic type arguments.
    /// </summary>
    /// <param name="type">Type to be checked.</param>
    /// <param name="interfaceType">Interface to be checked if it is implemented.</param>
    /// <param name="genericTypeArguments">Arguments of interface to be checked.</param>
    /// <returns>True if type implements generic interface, otherwise false.</returns>
    public static bool ImplementsGenericInterface(
        this Type type,
        Type interfaceType,
        params Type[] genericTypeArguments)
    {
        Type genericInterface = GetGenericInterface(type, interfaceType);

        if (genericInterface != null && genericTypeArguments.Any())
        {
            return genericInterface.GenericTypeArguments.SequenceEqual(genericTypeArguments);
        }

        return genericInterface != null;
    }

    /// <summary>
    ///     Returns the implemented generic interface of the object. If the interface is not implemented, null is returned.
    /// </summary>
    /// <param name="type">Type of the object from which the interface is returned.</param>
    /// <param name="genericInterface">Generic interface to return.</param>
    /// <returns>Type of generic interface, if none is implemented, null is returned.</returns>
    public static Type GetGenericInterface(this Type type, Type genericInterface)
    {
        return type.IsGenericType(genericInterface)
            ? type
            : type.GetInterfaces().FirstOrDefault(t => t.IsGenericType(genericInterface));
    }

    /// <summary>
    ///     Check if the type is type of the given generic type.
    /// </summary>
    /// <param name="type">Type to be checked.</param>
    /// <param name="genericType">Generic type to be checked if it is implemented.</param>
    /// <returns>True if the generic type is implemented, otherwise false.</returns>
    public static bool IsGenericType(this Type type, Type genericType)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }

    /// <summary>
    ///     Checks if <paramref name="derivedType" /> is assignable from <paramref name="baseType" />. Both input parameters
    ///     must contain generic arguments. Otherwise it will return <c>false</c>.
    /// </summary>
    /// <remarks>Only works with <see cref="IDictionary{TKey,TValue}" /> or <see cref="IList{T}" /> types.</remarks>
    /// <param name="derivedType">The derived type.</param>
    /// <param name="baseType">The possible base type of <paramref name="derivedType" /></param>
    /// <returns>
    ///     A boolean value indicating if the generic type <paramref name="derivedType" /> is a derived type of
    ///     <paramref name="baseType" />.
    /// </returns>
    public static bool IsAssignableFromGeneric(
        this Type derivedType,
        Type baseType)
    {
        // special case arrays
        if (IsDerivedArrayAndCompatibleToBase(derivedType, baseType))
        {
            return true;
        }

        if (derivedType == null
            || baseType == null
            || !derivedType.IsGenericType
            || !baseType.IsGenericType)
        {
            return false;
        }

        Type[] genericArgumentsOfDerived = derivedType.GetGenericArguments();
        Type[] genericArgumentsOfBase = baseType.GetGenericArguments();

        if (genericArgumentsOfDerived.Length != genericArgumentsOfBase.Length)
        {
            return false;
        }

        return AreBothSameGenericList(
                derivedType,
                genericArgumentsOfDerived,
                baseType,
                genericArgumentsOfBase)
            || AreBothSameGenericDictionary(
                derivedType,
                genericArgumentsOfDerived,
                baseType,
                genericArgumentsOfBase);
    }
}
