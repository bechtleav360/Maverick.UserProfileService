using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     <see cref="Type" /> related extension methods.
/// </summary>
public static class TypeExtension
{
    /// <summary>
    ///     Creates an instance of the specified type using the first constructor with args from service provider.
    /// </summary>
    /// <typeparam name="TType">Type to cast the newly created object.</typeparam>
    /// <param name="typeInfo">Type to create an instance from.</param>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <returns>A reference to the newly created object.</returns>
    public static TType CreateInstance<TType>(this Type typeInfo, IServiceProvider serviceProvider)
    {
        ConstructorInfo constructor = typeInfo.GetConstructors()?.FirstOrDefault();

        if (constructor != null)
        {
            object[] args = constructor
                .GetParameters()
                .Select(o => serviceProvider.GetService(o.ParameterType))
                .ToArray();

            return (TType)Activator.CreateInstance(typeInfo, args);
        }

        return (TType)Activator.CreateInstance(typeInfo);
    }

    /// <summary>
    ///     Check if given type implements abstract generic class with given type arguments.
    /// </summary>
    /// <param name="type">Type to be checked.</param>
    /// <param name="genericBaseClassType">Abstract class to be checked whether the <paramref name="type" /> derives from it.</param>
    /// <param name="genericTypeArguments">Generic type arguments of <paramref name="genericBaseClassType" />.</param>
    /// <returns></returns>
    public static bool HasGenericBaseClass(
        this Type type,
        Type genericBaseClassType,
        params Type[] genericTypeArguments)
    {
        return type.BaseType?.IsGenericType == true
            && type.BaseType.GetGenericTypeDefinition() == genericBaseClassType
            && type.BaseType.GenericTypeArguments.SequenceEqual(genericTypeArguments);
    }

    /// <summary>
    ///     Check if the given type is generic and checks if the generic type definition fits to the given generic type.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <param name="genericTypeDefinition">Generic type definition the definition fits to.</param>
    /// <returns>True if fits, otherwise false.</returns>
    public static bool IsGenericType(this Type type, Type genericTypeDefinition)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
    }

    /// <summary>
    ///     Returns a list of instance types and related service types that derives from a
    ///     <paramref name="genericInterfaceType" />.
    /// </summary>
    /// <param name="instanceAssembly">
    ///     An optional assembly where the instance types can be found. If <c>null</c>, the assembly
    ///     of <paramref name="genericInterfaceType" /> will be used.
    /// </param>
    /// <param name="genericInterfaceType">The generic interface the types to be found will be derived from.</param>
    /// <returns>A list of tuples containing instance and related service types to be added to a service collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="genericInterfaceType" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="genericInterfaceType" /> is not a generic type.</exception>
    public static List<(Type instanceType, Type serviceType)> GetInstanceTypesForDependencyInjection(
        this Type genericInterfaceType,
        Assembly instanceAssembly = null)
    {
        if (genericInterfaceType == null)
        {
            throw new ArgumentNullException(nameof(genericInterfaceType));
        }

        if (!genericInterfaceType.IsGenericType)
        {
            throw new ArgumentException(
                "The provided interface type must be a generic type, but is not.",
                nameof(genericInterfaceType));
        }

        Assembly assembly = instanceAssembly
            ?? genericInterfaceType.Assembly;

        return assembly
            .GetTypes()
            .Where(
                t => t.IsClass
                    && !t.IsAbstract)
            .Select(
                t =>
                    (t, t.GetInterfaces()
                        .FirstOrDefault(
                            i => i.IsGenericType
                                && i.GetGenericTypeDefinition()
                                == genericInterfaceType)))
            .Where(tuple => tuple.Item2 != null)
            .ToList();
    }
}
